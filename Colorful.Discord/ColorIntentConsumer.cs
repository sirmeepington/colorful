using Colorful.Common;
using DSharpPlus;
using DSharpPlus.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Colorful.Discord
{
    /// <summary>
    /// Mass Transit consumer for hte <see cref="IColorIntent"/> message type.
    /// </summary>
    public class ColorIntentConsumer : IConsumer<IColorIntent>
    {
        private readonly DiscordClient _client;
        private readonly ILogger<ColorIntentConsumer> _logger;

        public ColorIntentConsumer(DiscordClient client, ILogger<ColorIntentConsumer> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Consumes the given <see cref="IColorIntent"/> object.
        /// </summary>
        /// <param name="context">The consumer context.</param>
        public async Task Consume(ConsumeContext<IColorIntent> context)
        {
            try
            {
                IColorIntent msg = context.Message;
                Color color = msg.Color;
                IntentContext intent = new IntentContext();
                _logger.LogInformation("Received new color intent message.");

                try
                {
                    intent.Guild = await _client.GetGuildAsync(msg.Guild);
                    intent.Member = await intent.Guild.GetMemberAsync(msg.UserId);
                }
                catch
                {
                    _logger.LogError("Failed to find guild or member: {guild},{member}", msg.Guild, msg.UserId);
                    SentrySdk.CaptureMessage($"Failed to find provided guild or member: {msg.Guild}/{msg.UserId}");
                    return;
                }

                if (!await ValidatePermissions(intent))
                    return;


                (bool created, DiscordRole role) = await GetOrCreateRole(color, intent.Guild);
                intent.Role = role;
                intent.RoleCreated = created;
                if (created)
                {
                    _logger.LogInformation("Created new color role: {color} in guild.", color);
                }

                await RemovePreviousRoles(intent.Member);
                await AssignNewRole(color, intent);
                await CleanGuildRoles(intent.Guild);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Failed to assign color:");
            }
        }

        private async Task CleanGuildRoles(DiscordGuild guild)
        {
            _logger.LogInformation("Cleaning up color roles in guild {guild}", guild.Id);
            List<DiscordRole> allColorRoles = guild.Roles.Values
                .Where(x => Color.HEX_COLOR_REGEX.IsMatch(x.Name))
                .ToList();

            var members = await guild.GetAllMembersAsync();

            List<DiscordRole> allUsedRoles = members
                .SelectMany(m => m.Roles)
                .Where(x => Color.HEX_COLOR_REGEX.IsMatch(x.Name))
                .ToList();

            IEnumerable<DiscordRole> unusedRoles = allColorRoles.Except(allUsedRoles);

            foreach (var r in unusedRoles)
            {
                try
                {
                    await r.DeleteAsync();
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex,"Failed to clean guild roles for {guildId}", guild.Id);
                    SentrySdk.CaptureException(ex);
                }
                catch (DSharpPlus.Exceptions.UnauthorizedException)
                {
                    _logger.LogWarning("Attempted to clean roles in {guild} without Manage Roles permission.", guild.Id);
                    SentrySdk.CaptureMessage($"Attempted to remove role without permission: {r} in {guild.Name} ({guild.Id})", SentryLevel.Warning);
                    continue;
                }
            }
            _logger.LogInformation("Cleaned up {cleanedCount} roles in guild {guildId}", unusedRoles.Count(), guild.Id);
        }

        /// <summary>
        /// Assigns the color role for <paramref name="color"/> to the user.
        /// </summary>
        /// <param name="color">The color to assign the role for.</param>
        /// <param name="context">The intent's context.</param>
        /// <param name="colorRoles">A list of color roles to find the previous role.</param>
        private async Task AssignNewRole(Color color, IntentContext context)
        {
            await context.Member.GrantRoleAsync(context.Role);
            _logger.LogInformation("Assigned color role {color} to {user}", color.ToString(), context.Member.Id);
        }

        /// <summary>
        /// Validates the permissions the bot has.
        /// <br/>
        /// Returns whether the bot can manage roles.
        /// </summary>
        /// <param name="context">The intent context for sending a warning.</param>
        /// <returns>Whether the bot has the right permissions or not.</returns>
        private async Task<bool> ValidatePermissions(IntentContext context)
        {
            var perms = context.Guild.Permissions;
            bool canManageRoles = perms == null ? await AnyRoleCanManageRoles(context) : perms.Value.HasPermission(Permissions.ManageRoles);
            if (!canManageRoles)
            {
                _logger.LogWarning("Manage Roles permission missing in {guild}. Role cannot be created.",context.Guild.Id);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if any of the roles that the bot has contains 
        /// either the <see cref="Permissions.ManageRoles"/> or 
        /// <see cref="Permissions.Administrator"/> permissions.
        /// <br/>
        /// This is used when the bot's role is not accessible,
        /// e.g. when the invite is changed to not specify permissions.
        /// </summary>
        /// <param name="context">The <see cref="IntentContext"/> for
        /// this color intent.</param>
        /// <returns>Whether or not any role the bot has 
        /// can manage roles.</returns>
        private async Task<bool> AnyRoleCanManageRoles(IntentContext context)
        {
            var mem = await context.Guild.GetMemberAsync(_client.CurrentUser.Id);
            bool hasManageRoles = false;
            foreach (var role in mem.Roles)
            {
                if (role.CheckPermission(Permissions.ManageRoles) == PermissionLevel.Allowed
                    || role.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed)
                {
                    hasManageRoles = true;
                    break;
                }
            }
            return hasManageRoles;
        }

        private async Task<List<DiscordRole>> RemovePreviousRoles(DiscordMember member)
        {
            List<DiscordRole> colorRoles = member.Roles.Where(x => Color.HEX_COLOR_REGEX.IsMatch(x.Name)).ToList();
            foreach (DiscordRole cRole in colorRoles)
            {
                try
                {
                    await member.RevokeRoleAsync(cRole);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"Failed to remove previous role: {roleName} in guild {guildId}",cRole.Name,member.Guild.Id);
                    SentrySdk.CaptureException(ex);
                }
            }
            return colorRoles;
        }

        private async Task<(bool created, DiscordRole)> GetOrCreateRole(Color color, DiscordGuild guild)
        {
            DiscordRole role = guild.Roles.Values.FirstOrDefault(x => x.Name == color.Hex);
            if (role != null)
            {
                return (false, role);
            }

            try
            {
                role = await guild.CreateRoleAsync(
                    name: color.Hex,
                    color: new DiscordColor(color.Red, color.Green, color.Blue),
                    reason: "Color Role"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create role for color {color} in guild {guildId}", color, guild.Id);
                SentrySdk.CaptureException(ex);
                return (false, null);
            }

            var botMember = await guild.GetMemberAsync(_client.CurrentUser.Id);
            await role.ModifyPositionAsync(botMember.Hierarchy - 1);

            return (true, role);
        }
    }
}

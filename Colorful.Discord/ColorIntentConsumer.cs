using Colorful.Common;
using DSharpPlus;
using DSharpPlus.Entities;
using MassTransit;
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
        private DiscordClient _client;

        public ColorIntentConsumer(DiscordClient client)
        {
            _client = client;
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

                try
                {
                    intent.Guild = await _client.GetGuildAsync(msg.Guild);
                    intent.Member = await intent.Guild.GetMemberAsync(msg.UserId);
                }
                catch
                {
                    Console.WriteLine($"Can't find guild or member specified: {msg.Guild} - {msg.UserId}");
                    return;
                }

                intent = await FindDiscordReference(msg, intent);

                if (!await ValidatePermissions(intent))
                    return;


                (bool created, DiscordRole role) = await GetOrCreateRole(color, intent.Guild);
                intent.Role = role;
                intent.RoleCreated = created;
                if (intent.Role == null && intent.DiscordReferenceFound)
                {
                    await intent.Channel.SendMessageAsync($"Cannot create color role `{color.Hex}`. Check that I can create roles, etc.");
                    return;
                }

                List<DiscordRole> colorRoles = await RemovePreviousRoles(intent.Member);
                await AssignNewRole(color, intent, colorRoles);
            }
            catch (Exception ex)
            {
                Console.WriteLine("cant do it big man");
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<IntentContext> FindDiscordReference(IColorIntent msg, IntentContext intent)
        {
            if (msg.ChannelId != 0 && msg.MessageId != 0)
            {
                try
                {
                    intent.Channel = await _client.GetChannelAsync(msg.ChannelId);
                    intent.Message = await intent.Channel.GetMessageAsync(msg.MessageId);
                }
                catch
                {
                    Console.WriteLine("Can't find the channel or message the intent was created from. Returning to silent.");
                }
            }
            return intent;
        }

        /// <summary>
        /// Assigns the color role for <paramref name="color"/> to the user.
        /// </summary>
        /// <param name="color">The color to assign the role for.</param>
        /// <param name="context">The intent's context.</param>
        /// <param name="colorRoles">A list of color roles to find the previous role.</param>
        private async Task AssignNewRole(Color color, IntentContext context, List<DiscordRole> colorRoles)
        {
            await context.Member.GrantRoleAsync(context.Role);
            if (context.DiscordReferenceFound)
            {
                string create = context.RoleCreated ? $"Added color role for `{color.Hex}` to the server." : "";
                DiscordRole lastColor = colorRoles.FirstOrDefault();
                string replace = lastColor == null ? "" : $"Replaced from `{lastColor.Name}`";
                await context.Message.RespondAsync($"{create} Your color has been updated to `{color.Hex}`, {context.Member.Mention}. {replace}");
            }
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
            if (!perms.HasValue || perms.Value.HasPermission(Permissions.ManageRoles))
            {
                Console.WriteLine($"Can't Manage Roles in {context.Guild.Name} so I can't create the color role.");
                return false;
            }
            return true;
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
                    Console.WriteLine($"Failed to remove previous role: {cRole.Name}. Msg: {ex.Message}");
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
            catch
            {
                Console.WriteLine("Failed to create role for color " + color + " in guild: " + guild.Name);
                return (false, null);
            }

            var botMember = await guild.GetMemberAsync(_client.CurrentUser.Id);
            await role.ModifyPositionAsync(botMember.Hierarchy - 1);

            return (true, role);
        }
    }
}

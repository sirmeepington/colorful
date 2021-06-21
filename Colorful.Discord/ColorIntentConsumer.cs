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
                DiscordGuild guild;
                DiscordMember member;

                try
                {
                    guild = await _client.GetGuildAsync(msg.Guild);
                    member = await guild.GetMemberAsync(msg.UserId);
                } catch
                {
                    Console.WriteLine($"Can't find guild or member specified: {msg.Guild} - {msg.UserId}");
                    return;
                }

                DiscordChannel channel = null;
                DiscordMessage message = null;

                bool canFindDiscord = msg.ChannelId != 0 && msg.MessageId != 0;
                if (canFindDiscord)
                {
                    try
                    {
                        channel = await _client.GetChannelAsync(msg.ChannelId);
                        message = await channel.GetMessageAsync(msg.MessageId);
                    } catch
                    {
                        Console.WriteLine("Can't find the channel or message the intent was created from. Returning to silent.");
                        canFindDiscord = false;
                    }
                }

                if (!guild.Permissions.Value.HasPermission(Permissions.ManageRoles))
                {
                    if (canFindDiscord)
                    {
                        await message.RespondAsync("I can't manage roles. Please fix this.");
                    }
                    Console.WriteLine($"Can't Manage Roles in {guild.Name} so I can't create the color role {color}");
                    return;
                }


                (bool created, DiscordRole role) = await GetOrCreateRole(color, guild);
                if (role == null && canFindDiscord)
                {
                    await channel.SendMessageAsync($"Cannot create color role `{color.Hex}`. Check that I can create roles, etc.");
                    return;
                }

                List<DiscordRole> colorRoles = member.Roles.Where(x => Color.HEX_COLOR_REGEX.IsMatch(x.Name)).ToList();
                foreach (DiscordRole cRole in colorRoles)
                {
                    try
                    {
                        await member.RevokeRoleAsync(cRole);
                    } catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to remove previous role: {cRole.Name}. Msg: {ex.Message}");
                    }
                }

                await member.GrantRoleAsync(role);
                if (canFindDiscord)
                {
                    string create = created ? $"Added color role for `{color.Hex}` to the server." : "";
                    DiscordRole lastColor = colorRoles.FirstOrDefault();
                    string replace = lastColor == null ? "" : $"Replaced from `{lastColor.Name}`";
                    await message.RespondAsync($"{create} Your color has been updated to `{color.Hex}`, {member.Mention}. {replace}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("cant do it big man");
                Console.WriteLine(ex.Message);
            }
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

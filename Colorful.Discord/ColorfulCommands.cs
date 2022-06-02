using Colorful.Common;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MassTransit;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading.Tasks;

using Color = Colorful.Common.Color;

namespace Colorful.Discord
{
    /// <summary>
    /// Command class for use with DSharpPlus.CommandsNext
    /// </summary>
    public class ColorfulCommands : ApplicationCommandModule
    {
        /// <summary>
        /// Mass Transit Bus created via DI.
        /// </summary>
        public IBusControl Bus { private get; set; }

        public ILogger<ColorfulCommands> Logger { private get; set; }

        /// <summary>
        /// Assigns a role color to the user from the <paramref name="hexColor"/> specified.
        /// </summary>
        /// <param name="ctx">The command's context, populated via DSharpPlus</param>
        /// <param name="hexColor">A hex color string that fits the 
        /// <see cref="Color.HEX_COLOR_REGEX"/> regex.</param>
        [SlashCommand(name: "role", description: "Gives / creates a color role from the given hex. Moves new roles as high as possible.")]
        [SlashRequireBotPermissions(Permissions.ManageRoles)]
        public async Task RoleColor(InteractionContext ctx, [Option("color", "Hex color code including the hashtag.")] string hexColor)
        {
            if (!await CheckValidColor(ctx, hexColor))
            {
                return;
            }
            Color color = new Color(hexColor);

            if (color.Hex == "#000000") // Discord does NOT like #000000, it resets it to no color.
                color = new Color("#111111");


            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                new DiscordInteractionResponseBuilder().WithContent("Setting role. Please wait.").AsEphemeral());

            DiscordMessage msg = await ctx.GetOriginalResponseAsync();

            Logger.LogInformation("Sending color intent for color {color} in guild {guild} for member {member}",color,ctx.Guild.Id,ctx.User.Id);

            await Bus.Publish<IColorIntent>(new ColorIntent()
            {
                Guild = ctx.Guild.Id,
                UserId = ctx.User.Id,
                Color = color,
                ChannelId = ctx.Channel.Id,
                MessageId = msg.Id
            });
            

        }

        /// <summary>
        /// Shows a color image from the <paramref name="hexColor"/> specified.
        /// </summary>
        /// <param name="ctx">The command context, populated via DSharpPlus</param>
        /// <param name="hexColor">A hex string that matches the
        /// <see cref="Color.HEX_COLOR_REGEX"/> regex.</param>
        [SlashCommand(name: "color", description: "Shows the color given from the hex code.")]
        [SlashRequireBotPermissions(DSharpPlus.Permissions.AttachFiles)]
        public async Task ShowColor(InteractionContext ctx, [Option("color", "Hex color code including the hashtag.")] string hexColor) 
        {
            if (!await CheckValidColor(ctx, hexColor))
            {
                return;
            }

            await ctx.DeferAsync();
            Color color = new Color(hexColor);

            using Image<Rgba32> image = new Image<Rgba32>(64, 64, new Rgba32(color.Red, color.Green, color.Blue));
            using Stream stream = new MemoryStream();

            await image.SaveAsync(stream,new PngEncoder());

            stream.Seek(0, SeekOrigin.Begin);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddFile($"color_{hexColor}.png", stream)
                .WithContent($"Here's `{color.Hex}` (`{color.Red}`, `{color.Green}`, `{color.Blue}`):"));
        }

        /// <summary>
        /// Checks that the provided <paramref name="hexColor"/> is valid.
        /// <br/>
        /// Creates a response in the <paramref name="ctx"/> if it isn't.
        /// <br/>
        /// This method requires the hashtag in the hex color for it to be valid.
        /// </summary>
        /// <param name="ctx">The <see cref="InteractionContext"/> of the slash command</param>
        /// <param name="hexColor">The hex color to check against <see cref="Color.HEX_COLOR_REGEX"/></param>
        /// <returns>Whether the given <paramref name="hexColor"/> is a valid hex color.</returns>

        private async Task<bool> CheckValidColor(InteractionContext ctx, string hexColor)
        {
            if (!Color.HEX_COLOR_REGEX.IsMatch(hexColor))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Please specify a hex color, hashtag included (`#FFFFFF`).").AsEphemeral());
                return false;
            }
            return true;
        }
    }

}
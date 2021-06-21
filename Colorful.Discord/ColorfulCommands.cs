using Colorful.Common;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MassTransit;
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
    public class ColorfulCommands : BaseCommandModule
    {
        /// <summary>
        /// Mass Transit Bus created via DI.
        /// </summary>
        public IBusControl Bus { private get; set; }

        /// <summary>
        /// Assigns a role color to the user from the <paramref name="hexColor"/> specified.
        /// </summary>
        /// <param name="ctx">The command's context, populated via DSharpPlus</param>
        /// <param name="hexColor">A hex color string that fits the 
        /// <see cref="Color.HEX_COLOR_REGEX"/> regex.</param>
        [Command("role")]
        [Aliases("rolecolor","colorrole")]
        [Description("Gives a color role from the given hex. Creates the role if it does not exist. Moves new roles as high as possible.")]
        [Cooldown(1, 30d, CooldownBucketType.User)]
        public async Task RoleColor(CommandContext ctx, string hexColor)
        {
            if (!Color.HEX_COLOR_REGEX.IsMatch(hexColor))
            {
                await ctx.RespondAsync("Please specify a hex color, hashtag included (`#FFFFFF`).");
                return;
            }
            Color color = new Color(hexColor);

            if (color.Hex == "#000000") // Discord does NOT like #000000, it resets it to no color.
                color = new Color("#111111");

            await Bus.Publish<IColorIntent>(new ColorIntent() 
            {
                Guild = ctx.Guild.Id,
                UserId = ctx.User.Id,
                Color = color,
                ChannelId = ctx.Channel.Id,
                MessageId = ctx.Message.Id
            });
        }

        /// <summary>
        /// Shows a color image from the <paramref name="hexColor"/> specified.
        /// </summary>
        /// <param name="ctx">The command context, populated via DSharpPlus</param>
        /// <param name="hexColor">A hex string that matches the
        /// <see cref="Color.HEX_COLOR_REGEX"/> regex.</param>
        [Command("color")]
        [Description("Shows the color given from the hex code.")]
        [Cooldown(1, 10d, CooldownBucketType.User)]
        public async Task ShowColor(CommandContext ctx, string hexColor) 
        {
            if (!Color.HEX_COLOR_REGEX.IsMatch(hexColor))
            {
                await ctx.RespondAsync("Please specify a hex color, hashtag included (`#FFFFFF`).");
                return;
            }
            Color color = new Color(hexColor);

            using Image<Rgba32> image = new Image<Rgba32>(64, 64, new Rgba32(color.Red, color.Green, color.Blue));
            using Stream stream = new MemoryStream();

            await image.SaveAsync(stream,new PngEncoder());

            stream.Seek(0, SeekOrigin.Begin);

            DiscordMessageBuilder builder = new DiscordMessageBuilder();
            builder.WithFile($"{color.Hex}.png", stream);
            builder.WithContent($"Here's `{color.Hex}` (`{color.Red}`, `{color.Green}`, `{color.Blue}`):");

            await ctx.RespondAsync(builder);
        }

    }
}
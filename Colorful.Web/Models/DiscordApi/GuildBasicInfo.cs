namespace Colorful.Web.Models.DiscordApi
{
    /// <summary>
    /// An object containing basic information about a guild.
    /// </summary>
    public class GuildBasicInfo
    {

        /// <summary>
        /// The guild's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The guild's icon url.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// The discord id of the guild.
        /// </summary>
        public ulong Id { get; set; }

    }
}

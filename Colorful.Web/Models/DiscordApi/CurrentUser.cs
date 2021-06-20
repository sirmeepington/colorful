namespace Colorful.Web.Models.DiscordApi
{
    /// <summary>
    /// A model object to describe the current logged in user.
    /// </summary>
    public class CurrentUser
    {
        /// <summary>
        /// Their discord id
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// Their avatar url
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Their username, without the discriminatior.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Their discriminator.
        /// </summary>
        public ushort Discriminator { get; set; }

    }
}

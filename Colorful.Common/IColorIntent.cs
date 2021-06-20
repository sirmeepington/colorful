namespace Colorful.Common
{
    /// <summary>
    /// An interface type for messages to show the intent
    /// of changing a color.
    /// </summary>
    public interface IColorIntent
    {
        /// <summary>
        /// The color to change to.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// The id of the guild to get the role for.
        /// </summary>
        ulong Guild { get; set; }

        /// <summary>
        /// The user who to give the color to.
        /// </summary>
        ulong UserId { get; set; }

        /// <summary>
        /// The channel id if this was triggered from a message.
        /// </summary>
        ulong ChannelId { get; set; }

        /// <summary>
        /// The individual message id that this was triggered from, if any.
        /// </summary>
        ulong MessageId { get; set; }
    }
}
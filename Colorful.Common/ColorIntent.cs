using System;

namespace Colorful.Common
{
    /// <summary>
    /// An object implementing the <see cref="IColorIntent"/> message
    /// interface.
    /// </summary>
    public class ColorIntent : IColorIntent
    {
        /// <inheritdoc/>
        public ulong Guild { get; set; }

        /// <inheritdoc/>
        public Color Color { get; set; }

        /// <inheritdoc/>
        public ulong UserId { get; set; }

        /// <inheritdoc/>
        public ulong MessageId { get; set; }

        /// <inheritdoc/>
        public ulong ChannelId { get; set; }
    }
}

using System;

namespace Colorful.Common
{
    public class ColorIntent : IColorIntent
    {

        public ulong Guild { get; set; }

        public Color Color { get; set; }

        public ulong User { get; set; }

        public ulong Message { get; set; }

        public ulong Channel { get; set; }
    }
}

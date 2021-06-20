using System.Globalization;
using System.Text.RegularExpressions;

namespace Colorful.Common
{
    /// <summary>
    /// An object for an RGB color.
    /// </summary>
    public class Color
    {
        /// <summary>
        /// A <see cref="Regex"/> statement for a hex color.
        /// </br>
        /// An optional hashtag, with six characters, ranging from 0-9 and a-f. Case insensitive.
        /// </summary>

        public static readonly Regex HEX_COLOR_REGEX = new Regex(@"^#?([A-Fa-f0-9]{6})$");

        /// <summary>
        /// The amount of red.
        /// </summary>
        public byte Red { get; set; }

        /// <summary>
        /// The amount of green.
        /// </summary>
        public byte Green { get; set; }

        /// <summary>
        /// The amount of blue
        /// </summary>
        public byte Blue { get; set; }

        /// <summary>
        /// A hex code representation of this color. Matches the
        /// <see cref="HEX_COLOR_REGEX"/>
        /// </summary>
        public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}";

        public static implicit operator System.Drawing.Color(Color c) => System.Drawing.Color.FromArgb(c.Red,c.Green,c.Blue);


        public Color()
        {

        }

        /// <summary>
        /// Creates a <see cref="Color"/> object from specified <paramref name="red"/>,
        /// <paramref name="green"/> and <paramref name="blue"/> values.
        /// </summary>
        /// <param name="red">The red amount.</param>
        /// <param name="green">The red amount.</param>
        /// <param name="blue">The red amount.</param>
        public Color(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Creates a <see cref="Color"/> object from the given <paramref name="colorHex"/>
        /// value.
        /// <br/>
        /// Throws a <see cref="System.ArgumentException"/> if the 
        /// </summary>
        /// <param name="colorHex">A hex string which matches <see cref="HEX_COLOR_REGEX"/></param>
        /// <exception cref="System.ArgumentException">Thrown if
        /// <paramref name="colorHex"/> does not match 
        /// <see cref="HEX_COLOR_REGEX"/>.</exception>
        public Color(string colorHex)
        {
            if (!HEX_COLOR_REGEX.IsMatch(colorHex))
                throw new System.ArgumentException("Hex string parameter must match the hex regex.");

            string clr = colorHex.Replace("#", string.Empty);
            Red = byte.Parse(clr.Substring(0, 2), NumberStyles.HexNumber);
            Green = byte.Parse(clr.Substring(2, 2), NumberStyles.HexNumber);
            Blue = byte.Parse(clr.Substring(4, 2), NumberStyles.HexNumber);
        }

        /// <summary>
        /// Creates a <see cref="Color"/> object from independant
        /// <paramref name="red"/>, <paramref name="green"/> and <paramref name="blue"/> 
        /// string values.
        /// <para/>
        /// Parses to a <see cref="byte"/> via <see cref="byte.Parse(string)"/>.
        /// </summary>
        /// <param name="red">The red value</param>
        /// <param name="blue">The blue value</param>
        /// <param name="green">The green value</param>
        public Color(string red, string blue, string green)
        {
            Red = byte.Parse(red);
            Blue = byte.Parse(blue);
            Green = byte.Parse(green);
        }

        public override string ToString()
        {
            return $"Color: {Hex} ({Red}, {Green}, {Blue})";
        }
    }
}
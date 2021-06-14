using System.Globalization;

namespace Colorful.Common
{
    public class Color
    {
        public byte Red { get; set; }

        public byte Green { get; set; }

        public byte Blue { get; set; }

        public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}";

        public static implicit operator System.Drawing.Color(Color c) => System.Drawing.Color.FromArgb(c.Red,c.Green,c.Blue);


        public Color()
        {

        }

        public Color(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public Color(string colorHex)
        {
            string clr = colorHex.Replace("#", string.Empty);
            Red = byte.Parse(clr.Substring(0, 2), NumberStyles.HexNumber);
            Green = byte.Parse(clr.Substring(2, 2), NumberStyles.HexNumber);
            Blue = byte.Parse(clr.Substring(4, 2), NumberStyles.HexNumber);
        }

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
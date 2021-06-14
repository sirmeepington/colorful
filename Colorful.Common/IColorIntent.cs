namespace Colorful.Common
{
    public interface IColorIntent
    {
        Color Color { get; set; }

        ulong Guild { get; set; }

        ulong User { get; set; }

        ulong Channel { get; set; }

        ulong Message { get; set; }
    }
}
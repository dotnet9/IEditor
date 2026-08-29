using ImageMagick;

namespace IEditor.Core.Models;

public enum PhotoOutputFormat
{
    Png,
    Jpeg
}

public static class PhotoOutputFormatExtensions
{
    public static MagickFormat ToMagickFormat(this PhotoOutputFormat format) =>
        format switch
        {
            PhotoOutputFormat.Png => MagickFormat.Png,
            PhotoOutputFormat.Jpeg => MagickFormat.Jpeg,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

    public static string GetExtension(this PhotoOutputFormat format) =>
        format switch
        {
            PhotoOutputFormat.Png => ".png",
            PhotoOutputFormat.Jpeg => ".jpg",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
}

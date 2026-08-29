namespace IEditor.Core.Models;

public static class PhotoSizeCatalog
{
    public static PhotoSizeSpec OneInch(int dpi) => new(25, 35, dpi);

    public static PhotoSizeSpec TwoInch(int dpi) => new(35, 49, dpi);
}

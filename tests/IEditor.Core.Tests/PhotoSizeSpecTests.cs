using IEditor.Core.Models;
using Xunit;

namespace IEditor.Core.Tests;

public class PhotoSizeSpecTests
{
    [Fact]
    public void OneInchSizeConvertsToPixels()
    {
        var spec = PhotoSizeCatalog.OneInch(300);

        Assert.Equal(25d, spec.WidthMm);
        Assert.Equal(35d, spec.HeightMm);
        Assert.Equal(295, spec.WidthPx);
        Assert.Equal(413, spec.HeightPx);
    }

    [Fact]
    public void TwoInchSizeConvertsToPixels()
    {
        var spec = PhotoSizeCatalog.TwoInch(300);

        Assert.Equal(35d, spec.WidthMm);
        Assert.Equal(49d, spec.HeightMm);
        Assert.Equal(413, spec.WidthPx);
        Assert.Equal(579, spec.HeightPx);
    }
}

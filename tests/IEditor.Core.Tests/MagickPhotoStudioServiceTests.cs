using ImageMagick;
using IEditor.Core.Models;
using IEditor.Core.Services;
using Xunit;

namespace IEditor.Core.Tests;

public class MagickPhotoStudioServiceTests
{
    [Fact]
    public async Task ProcessAsync_CreatesExpectedOutputSize()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

        try
        {
            using var source = new MagickImage(MagickColors.Transparent, 120, 160);
            source.Write(sourcePath, MagickFormat.Png);

            var service = new MagickPhotoStudioService();
            var request = new PhotoProcessingRequest(
                sourcePath,
                new PhotoSizeSpec(25, 35, 300),
                RgbColor.White,
                PhotoOutputFormat.Png);

            var result = await service.ProcessAsync(request, TestContext.Current.CancellationToken);
            using var processed = new MagickImage(result.Data);

            Assert.Equal((uint)request.TargetSize.WidthPx, processed.Width);
            Assert.Equal((uint)request.TargetSize.HeightPx, processed.Height);
            Assert.True(processed.IsOpaque);
        }
        finally
        {
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }
    }
}

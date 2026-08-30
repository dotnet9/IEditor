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

    [Fact]
    public async Task ProcessAsync_RotationAndFlip_KeepExpectedOutputSize()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

        try
        {
            // 上半部分为不透明红色，下半部分为透明：旋转 180° 后底部应变为不透明红色。
            using var source = new MagickImage(MagickColors.Transparent, 120, 160);
            using var red = new MagickImage(MagickColors.Red, 120, 80);
            source.Composite(red, Gravity.North, CompositeOperator.Over);
            source.Write(sourcePath, MagickFormat.Png);

            var service = new MagickPhotoStudioService();
            var request = new PhotoProcessingRequest(
                sourcePath,
                new PhotoSizeSpec(25, 35, 300),
                RgbColor.White,
                PhotoOutputFormat.Png,
                92,
                RotationDegrees: 180,
                FlipHorizontal: true);

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

    [Fact]
    public async Task ProcessAsync_ZoomAndOffset_MoveContentWithinCanvas()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

        try
        {
            using var source = new MagickImage(MagickColors.Red, 100, 100);
            source.Write(sourcePath, MagickFormat.Png);

            var service = new MagickPhotoStudioService();
            var centeredRequest = new PhotoProcessingRequest(
                sourcePath,
                new PhotoSizeSpec(25, 35, 300),
                RgbColor.White,
                PhotoOutputFormat.Png,
                92,
                ZoomFactor: 0.5);

            var shiftedRequest = centeredRequest with { OffsetX = 60 };

            var centeredResult = await service.ProcessAsync(centeredRequest, TestContext.Current.CancellationToken);
            var shiftedResult = await service.ProcessAsync(shiftedRequest, TestContext.Current.CancellationToken);

            using var centered = new MagickImage(centeredResult.Data);
            using var shifted = new MagickImage(shiftedResult.Data);
            using var centeredPixels = centered.GetPixels();
            using var shiftedPixels = shifted.GetPixels();

            var sampleX = 110;
            var sampleY = (int)centered.Height / 2;

            Assert.True(IsRed(centeredPixels.GetPixel(sampleX, sampleY)));
            Assert.True(IsWhite(shiftedPixels.GetPixel(sampleX, sampleY)));
        }
        finally
        {
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }
    }

    [Fact]
    public async Task ProcessAsync_SmartCutout_RemovesBorderBackground()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

        try
        {
            using var source = new MagickImage(MagickColor.FromRgb(67, 142, 219), 120, 160);
            using var subject = new MagickImage(MagickColors.Red, 54, 74);
            source.Composite(subject, Gravity.Center, CompositeOperator.Over);
            source.Write(sourcePath, MagickFormat.Png);

            var service = new MagickPhotoStudioService();
            var request = new PhotoProcessingRequest(
                sourcePath,
                new PhotoSizeSpec(25, 35, 300),
                RgbColor.White,
                PhotoOutputFormat.Png,
                92,
                EnableSmartCutout: true);

            var result = await service.ProcessAsync(request, TestContext.Current.CancellationToken);
            using var processed = new MagickImage(result.Data);
            using var pixels = processed.GetPixels();

            var corner = pixels.GetPixel(0, 0);
            var center = pixels.GetPixel((int)processed.Width / 2, (int)processed.Height / 2);

            Assert.True(ToByte(corner.GetChannel(0)) > 240);
            Assert.True(ToByte(corner.GetChannel(1)) > 240);
            Assert.True(ToByte(corner.GetChannel(2)) > 240);

            Assert.True(ToByte(center.GetChannel(0)) > 180);
            Assert.True(ToByte(center.GetChannel(1)) < 100);
            Assert.True(ToByte(center.GetChannel(2)) < 100);
        }
        finally
        {
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }
    }

    private static bool IsRed(IPixel<ushort> pixel) =>
        ToByte(pixel.GetChannel(0)) > 200 &&
        ToByte(pixel.GetChannel(1)) < 80 &&
        ToByte(pixel.GetChannel(2)) < 80;

    private static bool IsWhite(IPixel<ushort> pixel) =>
        ToByte(pixel.GetChannel(0)) > 240 &&
        ToByte(pixel.GetChannel(1)) > 240 &&
        ToByte(pixel.GetChannel(2)) > 240;

    private static byte ToByte(ushort value) => (byte)Math.Clamp((int)Math.Round(value / 257d), 0, 255);
}

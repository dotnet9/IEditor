using IEditor.Core.Models;

namespace IEditor.Core.Services;

public interface IPhotoStudioService
{
    Task<PhotoProcessingResult> ProcessAsync(PhotoProcessingRequest request, CancellationToken cancellationToken = default);
}

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public interface IMediaTypeEditingService
{
    Task<Attempt<IMediaType?, ContentTypeOperationStatus>> CreateAsync(MediaTypeCreateModel model, Guid userKey);
}

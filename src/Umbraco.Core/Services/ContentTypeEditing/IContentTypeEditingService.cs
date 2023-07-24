using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public interface IContentTypeEditingService
{
    Task<Attempt<IContentType?, ContentTypeOperationStatus>> CreateAsync(ContentTypeCreateModel model, Guid performingUserId);
}

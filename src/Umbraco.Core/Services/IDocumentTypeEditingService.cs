using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing.Document;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IDocumentTypeEditingService
{
    Task<Attempt<IContentType, ContentTypeOperationStatus>> CreateAsync(DocumentTypeCreateModel model, Guid performingUserId);
}

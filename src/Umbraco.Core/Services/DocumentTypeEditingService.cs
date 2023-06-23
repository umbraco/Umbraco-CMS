using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class DocumentTypeEditingService : IDocumentTypeEditingService
{
    public Task<Attempt<IContentType, ContentTypeOperationStatus>> CreateAsync(DocumentTypeCreateModel model, Guid performingUserId) => throw new NotImplementedException();
}

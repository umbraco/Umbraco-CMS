using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IContentTypeReferenceService
{
    Task<PagedModel<Guid>> GetReferencedDocumentKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take);
}

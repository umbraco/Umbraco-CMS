using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class ContentTypeReferenceService : IContentTypeReferenceService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public ContentTypeReferenceService(IDocumentRepository documentRepository, ICoreScopeProvider coreScopeProvider)
    {
        _documentRepository = documentRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public Task<PagedModel<Guid>> GetReferencedDocumentKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        PagedModel<Guid> keys = _documentRepository.GetReferencingDocumentsByDocumentTypeKey(key);
        scope.Complete();

        return Task.FromResult(keys);
    }
}

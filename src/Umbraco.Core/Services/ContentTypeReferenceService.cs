using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class ContentTypeReferenceService : IContentTypeReferenceService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public ContentTypeReferenceService(IDocumentRepository documentRepository, IContentTypeRepository contentTypeRepository, ICoreScopeProvider coreScopeProvider)
    {
        _documentRepository = documentRepository;
        _contentTypeRepository = contentTypeRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public Task<PagedModel<Guid>> GetReferencedDocumentKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        PagedModel<Guid> keys = _documentRepository.GetReferencingDocumentsByDocumentTypeKey(key);
        scope.Complete();

        return Task.FromResult(keys);
    }

    public Task<PagedModel<Guid>> GetReferencedDocumentTypeKeysAsync(Guid key, CancellationToken cancellationToken,
        int skip, int take)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        PagedModel<Guid> keys = _contentTypeRepository.GetChildren(key);
        scope.Complete();

        return Task.FromResult(keys);
    }
}

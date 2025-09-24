using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class ContentTypeReferenceService : IContentTypeReferenceService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IDataTypeRepository _dataTypeRepository;

    public ContentTypeReferenceService(IDocumentRepository documentRepository, IContentTypeRepository contentTypeRepository, ICoreScopeProvider coreScopeProvider, IDataTypeRepository dataTypeRepository)
    {
        _documentRepository = documentRepository;
        _contentTypeRepository = contentTypeRepository;
        _coreScopeProvider = coreScopeProvider;
        _dataTypeRepository = dataTypeRepository;
    }

    public Task<PagedModel<Guid>> GetReferencedDocumentKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        PagedModel<Guid> keys = _documentRepository.GetReferencingDocumentsByDocumentTypeKey(key);
        scope.Complete();

        return Task.FromResult(keys);
    }

    public Task<PagedModel<Guid>> GetReferencedDocumentTypeKeysAsync(Guid key, CancellationToken cancellationToken, int skip, int take)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        PagedModel<Guid> keys = _contentTypeRepository.GetChildren(key);
        scope.Complete();

        return Task.FromResult(keys);
    }

    public Task<PagedModel<Guid>> GetReferencedElementsFromDataTypesAsync(Guid key, CancellationToken cancellationToken, int skip, int take)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        PagedModel<Guid> keys = _dataTypeRepository.GetBlockEditorsReferencingContentType(key, skip, take);
        scope.Complete();

        return Task.FromResult(keys);
    }
}

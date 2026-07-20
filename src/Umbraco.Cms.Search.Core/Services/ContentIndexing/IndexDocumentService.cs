using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Persistence;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

public class IndexDocumentService : IIndexDocumentService
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IIndexDocumentRepository _indexDocumentRepository;

    public IndexDocumentService(ICoreScopeProvider scopeProvider, IIndexDocumentRepository indexDocumentRepository)
    {
        _scopeProvider = scopeProvider;
        _indexDocumentRepository = indexDocumentRepository;
    }

    public async Task AddAsync(IndexDocument indexDocument)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _indexDocumentRepository.AddAsync(indexDocument);
        scope.Complete();
    }

    public async Task DeleteAsync(Guid[] ids, bool published)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _indexDocumentRepository.DeleteAsync(ids, published);
        scope.Complete();
    }

    public async Task<IndexDocument?> GetAsync(Guid id, bool published)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        IndexDocument? document = await _indexDocumentRepository.GetAsync(id, published);
        scope.Complete();

        return document;
    }

    public async Task DeleteAllAsync()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _indexDocumentRepository.DeleteAllAsync();
        scope.Complete();
    }

    public async Task DeleteCulturesAsync(IReadOnlyCollection<string> isoCodes)
    {
        long total;
        long currentPage = 1;
        const int pageSize = 1000;

        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        var changes = new List<(Guid Key, bool Published, IndexDocument? Updated)>();
        do
        {
            PagedModel<IndexDocument> page = await _indexDocumentRepository.GetPagedAsync(currentPage, pageSize);
            total = page.Total;

            foreach (IndexDocument document in page.Items)
            {
                IndexField[] fieldsToRemove = document.Fields
                    .Where(field => field.Culture is not null && isoCodes.InvariantContains(field.Culture))
                    .ToArray();

                if (fieldsToRemove.Any() is false)
                {
                    continue;
                }

                // Queue changes - we can't perform the updates here, it will interfere with the pagination.
                changes.Add(new(
                    document.Key,
                    document.Published,
                    fieldsToRemove.Length == document.Fields.Length
                        ? null
                        : new IndexDocument
                        {
                            Key = document.Key,
                            Published = document.Published,
                            Fields = document.Fields.Except(fieldsToRemove).ToArray(),
                        }));
            }
        }
        while (currentPage++ * pageSize <= total);

        // bulk delete keys based on publish status
        await DeleteDocumentsAsync(true);
        await DeleteDocumentsAsync(false);

        // insert all updates
        foreach (IndexDocument update in changes.Select(change => change.Updated).WhereNotNull())
        {
            await _indexDocumentRepository.AddAsync(update);
        }

        async Task DeleteDocumentsAsync(bool published)
        {
            Guid[] keys = changes
                .Where(change => change.Published == published)
                .Select(change => change.Key)
                .ToArray();
            if (keys.Length > 0)
            {
                await _indexDocumentRepository.DeleteAsync(keys, published);
            }
        }

        scope.Complete();
    }
}

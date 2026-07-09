using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Persistence;
using Umbraco.Cms.Search.Core.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing.Search;

// These tests are not concerned with DB-cached index data, so this is a no-op implementation
// of the document repository.
internal class NoopIndexDocumentRepository : IIndexDocumentRepository
{
    public Task AddAsync(IndexDocument indexDocument) => Task.CompletedTask;

    public Task<IndexDocument?> GetAsync(Guid id, bool published) => Task.FromResult<IndexDocument?>(null);

    public Task DeleteAsync(Guid[] ids, bool published) => Task.CompletedTask;

    public Task DeleteAllAsync() => Task.CompletedTask;

    public Task<PagedModel<IndexDocument>> GetPagedAsync(long currentPage, int pageSize)
        => Task.FromResult(new PagedModel<IndexDocument>());
}

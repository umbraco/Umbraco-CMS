using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Persistence;

namespace Umbraco.Cms.Search.Core.Persistence;

public interface IIndexDocumentRepository
{
    public Task AddAsync(IndexDocument indexDocument);

    public Task<IndexDocument?> GetAsync(Guid id, bool published);

    public Task DeleteAsync(Guid[] ids, bool published);

    public Task DeleteAllAsync();

    public Task<PagedModel<IndexDocument>> GetPagedAsync(long currentPage, int pageSize);
}

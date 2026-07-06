using Umbraco.Cms.Search.Core.Models.Persistence;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

public interface IIndexDocumentService
{
    Task AddAsync(IndexDocument indexDocument);

    Task DeleteAsync(Guid[] ids, bool published);

    Task<IndexDocument?> GetAsync(Guid id, bool published);

    Task DeleteAllAsync();

    Task DeleteCulturesAsync(IReadOnlyCollection<string> isoCodes);
}

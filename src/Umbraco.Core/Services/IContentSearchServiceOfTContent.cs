using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IContentSearchService<TContent>
    where TContent : class, IContentBase
{
    Task<PagedModel<TContent>> SearchChildrenAsync(
        string? query,
        Guid? parentId,
        Ordering? ordering,
        int skip = 0,
        int take = 100);
}

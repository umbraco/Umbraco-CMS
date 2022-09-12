using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Factories;

public class PagedViewModelFactory : IPagedViewModelFactory
{
    public PagedViewModel<T> Create<T>(IEnumerable<T> items, int skip, int take)
    {
        T[] enumerable = items as T[] ?? items.ToArray();
        return new PagedViewModel<T> { Total = enumerable.Length, Items = enumerable.Skip(skip).Take(take) };
    }
}

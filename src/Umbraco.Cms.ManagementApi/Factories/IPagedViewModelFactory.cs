using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Factories;

public interface IPagedViewModelFactory
{
    PagedViewModel<T> Create<T>(IEnumerable<T> items, int skip, int take);
}

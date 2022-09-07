using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Factories;

public interface IViewModelFactory
{
    PagedViewModel<T> Create<T>(IEnumerable<T> items, int skip, int take);
}

namespace Umbraco.Cms.ManagementApi.ViewModels.Pagination;

public class PagedViewModel<T>
{
    public long Total { get; set; }

    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    public static PagedViewModel<T> Empty() => new();
}

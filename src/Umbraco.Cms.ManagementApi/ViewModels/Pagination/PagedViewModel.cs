using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.ManagementApi.ViewModels.Pagination;

public class PagedViewModel<T>
{
    [Required]
    public long Total { get; set; }

    [Required]
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    public static PagedViewModel<T> Empty() => new();
}

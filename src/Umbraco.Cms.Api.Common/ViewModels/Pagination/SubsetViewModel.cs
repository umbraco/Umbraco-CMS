using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Common.ViewModels.Pagination;

public class SubsetViewModel<T>
{
    [Required]
    public long TotalBefore { get; set; }

    [Required]
    public long TotalAfter { get; set; }

    [Required]
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    public static SubsetViewModel<T> Empty() => new();
}

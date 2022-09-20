namespace Umbraco.New.Cms.Core.Models;

public class PagedModel<T>
{
    public PagedModel()
    {
    }

    public PagedModel(long total, IEnumerable<T> items)
    {
        Total = total;
        Items = items;
    }

    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();

    public long Total { get; init; }
}

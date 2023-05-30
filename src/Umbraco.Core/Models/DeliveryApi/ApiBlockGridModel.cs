namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiBlockGridModel
{
    public ApiBlockGridModel(int gridColumns, IEnumerable<ApiBlockGridItem> items)
    {
        GridColumns = gridColumns;
        Items = items;
    }

    public int GridColumns { get; }

    public IEnumerable<ApiBlockGridItem> Items { get; }
}

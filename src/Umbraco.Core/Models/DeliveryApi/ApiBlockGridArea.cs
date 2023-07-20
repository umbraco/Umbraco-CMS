namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiBlockGridArea
{
    public ApiBlockGridArea(string alias, int rowSpan, int columnSpan, IEnumerable<ApiBlockGridItem> items)
    {
        Alias = alias;
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
        Items = items;
    }

    public string Alias { get; }

    public int RowSpan { get; }

    public int ColumnSpan { get; }

    public IEnumerable<ApiBlockGridItem> Items { get; }
}

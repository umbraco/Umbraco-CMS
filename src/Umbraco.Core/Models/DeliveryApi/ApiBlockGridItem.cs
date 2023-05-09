namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiBlockGridItem : ApiBlockItem
{
    public ApiBlockGridItem(IApiElement content, IApiElement? settings, int rowSpan, int columnSpan, int areaGridColumns, IEnumerable<ApiBlockGridArea> areas)
        : base(content, settings)
    {
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
        AreaGridColumns = areaGridColumns;
        Areas = areas;
    }

    public int RowSpan { get; }

    public int ColumnSpan { get; }

    public int AreaGridColumns { get; }

    public IEnumerable<ApiBlockGridArea> Areas { get; }
}

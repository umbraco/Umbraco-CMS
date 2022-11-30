namespace Umbraco.Cms.Core.Headless;

public class HeadlessBlockGridItem : HeadlessBlockItem
{
    public HeadlessBlockGridItem(IHeadlessElement content, IHeadlessElement? settings, int rowSpan, int columnSpan, int areaGridColumns, IEnumerable<HeadlessBlockGridArea> areas)
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

    public IEnumerable<HeadlessBlockGridArea> Areas { get; }
}

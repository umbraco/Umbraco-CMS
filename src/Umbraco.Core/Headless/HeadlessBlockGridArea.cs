namespace Umbraco.Cms.Core.Headless;

public class HeadlessBlockGridArea
{
    public HeadlessBlockGridArea(string alias, int rowSpan, int columnSpan, IEnumerable<HeadlessBlockGridItem> items)
    {
        Alias = alias;
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
        Items = items;
    }

    public string Alias { get; }

    public int RowSpan { get; }

    public int ColumnSpan { get; }

    public IEnumerable<HeadlessBlockGridItem> Items { get; }
}

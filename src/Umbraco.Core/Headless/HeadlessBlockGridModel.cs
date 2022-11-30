namespace Umbraco.Cms.Core.Headless;

public class HeadlessBlockGridModel
{
    public HeadlessBlockGridModel(int gridColumns, IEnumerable<HeadlessBlockGridItem> items)
    {
        GridColumns = gridColumns;
        Items = items;
    }

    public int GridColumns { get; }

    public IEnumerable<HeadlessBlockGridItem> Items { get; }
}

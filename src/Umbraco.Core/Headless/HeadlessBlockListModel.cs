namespace Umbraco.Cms.Core.Headless;

public class HeadlessBlockListModel
{
    public HeadlessBlockListModel(IEnumerable<HeadlessBlockItem> items) => Items = items;

    public IEnumerable<HeadlessBlockItem> Items { get; }
}

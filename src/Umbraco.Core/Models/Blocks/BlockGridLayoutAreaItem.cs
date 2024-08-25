namespace Umbraco.Cms.Core.Models.Blocks;

public class BlockGridLayoutAreaItem
{
    public Guid Key { get; set; } = Guid.Empty;

    public BlockGridLayoutItem[] Items { get; set; } = Array.Empty<BlockGridLayoutItem>();

    public BlockGridLayoutAreaItem()
    { }

    public BlockGridLayoutAreaItem(Guid key)
        => Key = key;
}

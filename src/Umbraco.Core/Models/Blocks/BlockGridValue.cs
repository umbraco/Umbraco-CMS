namespace Umbraco.Cms.Core.Models.Blocks;

public class BlockGridValue : BlockValue<BlockGridLayoutItem>
{
    public override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockGrid;
}

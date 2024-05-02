namespace Umbraco.Cms.Core.Models.Blocks;

public class BlockListValue : BlockValue<BlockListLayoutItem>
{
    public override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockList;
}

namespace Umbraco.Cms.Core.Models.Blocks;

public class RichTextBlockValue : BlockValue<RichTextBlockLayoutItem>
{
    public override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.TinyMce;
}

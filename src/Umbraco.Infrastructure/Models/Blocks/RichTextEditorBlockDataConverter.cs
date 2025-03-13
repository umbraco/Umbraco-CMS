using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Data converter for blocks in the richtext property editor
/// </summary>
internal sealed class RichTextEditorBlockDataConverter : BlockEditorDataConverter
{
    public RichTextEditorBlockDataConverter()
        : base(Constants.PropertyEditors.Aliases.TinyMce)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference>? GetBlockReferences(JToken jsonLayout)
    {
        IEnumerable<RichTextBlockLayoutItem>? blockListLayout = jsonLayout.ToObject<IEnumerable<RichTextBlockLayoutItem>>();
        return blockListLayout?.Select(x => new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi)).ToList();
    }
}

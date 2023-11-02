namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Data converter for blocks in the richtext property editor
/// </summary>
public sealed class RichTextEditorBlockDataConverter : BlockEditorDataConverter<RichTextBlockValue, RichTextBlockLayoutItem>
{
    public RichTextEditorBlockDataConverter()
        : base(Constants.PropertyEditors.Aliases.TinyMce)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<RichTextBlockLayoutItem> layout)
        => layout.Select(x => new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi)).ToList();
}

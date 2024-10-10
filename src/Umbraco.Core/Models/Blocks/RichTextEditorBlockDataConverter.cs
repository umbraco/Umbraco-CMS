using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Data converter for blocks in the richtext property editor
/// </summary>
public sealed class RichTextEditorBlockDataConverter : BlockEditorDataConverter<RichTextBlockValue, RichTextBlockLayoutItem>
{
    [Obsolete("Use the constructor that takes IJsonSerializer. Will be removed in V15.")]
    public RichTextEditorBlockDataConverter()
        : base(Constants.PropertyEditors.Aliases.TinyMce)
    {
    }

    public RichTextEditorBlockDataConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<RichTextBlockLayoutItem> layout)
        => layout.Select(x => new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi)).ToList();
}

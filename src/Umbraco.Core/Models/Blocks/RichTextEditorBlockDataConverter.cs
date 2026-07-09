using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Data converter for blocks in the richtext property editor.
/// </summary>
public sealed class RichTextEditorBlockDataConverter : BlockEditorDataConverter<RichTextBlockValue, RichTextBlockLayoutItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RichTextEditorBlockDataConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public RichTextEditorBlockDataConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<RichTextBlockLayoutItem> layout)
        => layout.Select(x => new ContentAndSettingsReference(x.ContentKey, x.SettingsKey)).ToList();
}

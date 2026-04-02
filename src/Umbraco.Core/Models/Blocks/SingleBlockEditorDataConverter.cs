using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Data converter for the single block property editor.
/// </summary>
public class SingleBlockEditorDataConverter : BlockEditorDataConverter<SingleBlockValue, SingleBlockLayoutItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SingleBlockEditorDataConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public SingleBlockEditorDataConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<SingleBlockLayoutItem> layout)
        => layout.Select(x => new ContentAndSettingsReference(x.ContentKey, x.SettingsKey)).ToList();
}

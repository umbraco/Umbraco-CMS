using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Data converter for the single block property editor
/// </summary>
public class SingleBlockEditorDataConverter : BlockEditorDataConverter<SingleBlockValue, SingleBlockLayoutItem>
{
    public SingleBlockEditorDataConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<SingleBlockLayoutItem> layout)
        => layout.Select(x => new ContentAndSettingsReference(x.ContentKey, x.SettingsKey)).ToList();
}

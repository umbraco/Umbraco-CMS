// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Data converter for the block grid property editor
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] // TODO: Remove this for V11/V10.4
public class BlockGridEditorDataConverter : BlockEditorDataConverter
{
    private readonly IJsonSerializer _jsonSerializer;

    public BlockGridEditorDataConverter(IJsonSerializer jsonSerializer) : base(Cms.Core.Constants.PropertyEditors.Aliases.BlockGrid)
        => _jsonSerializer = jsonSerializer;

    protected override IEnumerable<ContentAndSettingsReference>? GetBlockReferences(JToken jsonLayout)
    {
        IEnumerable<BlockGridLayoutItem>? layouts = jsonLayout.ToObject<IEnumerable<BlockGridLayoutItem>>();
        if (layouts == null)
        {
            return null;
        }

        IList<ContentAndSettingsReference> ExtractContentAndSettingsReferences(BlockGridLayoutItem item)
        {
            var references = new List<ContentAndSettingsReference> { new(item.ContentUdi, item.SettingsUdi) };
            references.AddRange(item.Areas.SelectMany(area => area.Items.SelectMany(ExtractContentAndSettingsReferences)));
            return references;
        }

        ContentAndSettingsReference[] result = layouts.SelectMany(ExtractContentAndSettingsReferences).ToArray();
        return result;
    }
}

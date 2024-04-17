// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Handles the conversion of data for the block list property editor.
/// </summary>
public class BlockListEditorDataConverter : BlockEditorDataConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListEditorDataConverter"/> class with a default alias.
    /// </summary>
    public BlockListEditorDataConverter()
        : base(Constants.PropertyEditors.Aliases.BlockList)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListEditorDataConverter"/> class with a provided alias.
    /// </summary>
    /// <param name="propertyEditorAlias">The alias of the property editor.</param>
    public BlockListEditorDataConverter(string propertyEditorAlias)
        : base(propertyEditorAlias)
    {
    }

    /// <summary>
    /// Extracts block references from the provided JSON layout.
    /// </summary>
    /// <param name="jsonLayout">The JSON layout containing the block references.</param>
    /// <returns>A collection of <see cref="ContentAndSettingsReference"/> objects extracted from the JSON layout.</returns>
    protected override IEnumerable<ContentAndSettingsReference>? GetBlockReferences(JToken jsonLayout)
    {
        IEnumerable<BlockListLayoutItem>? blockListLayout = jsonLayout.ToObject<IEnumerable<BlockListLayoutItem>>();
        return blockListLayout?.Select(x => new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi)).ToList();
    }
}

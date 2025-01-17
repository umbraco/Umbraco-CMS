using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Represents a block list value.
/// </summary>
public class BlockListValue : BlockValue<BlockListLayoutItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListValue" /> class.
    /// </summary>
    public BlockListValue()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListValue" /> class.
    /// </summary>
    /// <param name="layouts">The layouts.</param>
    public BlockListValue(IEnumerable<BlockListLayoutItem> layouts)
        => Layout[PropertyEditorAlias] = layouts;

    /// <inheritdoc />
    [JsonIgnore]
    public override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockList;
}

using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Represents a block grid value.
/// </summary>
public class BlockGridValue : BlockValue<BlockGridLayoutItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridValue" /> class.
    /// </summary>
    public BlockGridValue()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridValue" /> class.
    /// </summary>
    /// <param name="layouts">The layouts.</param>
    public BlockGridValue(IEnumerable<BlockGridLayoutItem> layouts)
        => Layout[PropertyEditorAlias] = layouts;

    /// <inheritdoc />
    [JsonIgnore]
    public override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockGrid;
}

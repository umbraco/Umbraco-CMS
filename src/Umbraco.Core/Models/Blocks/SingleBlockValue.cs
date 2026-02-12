using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Represents a single block value.
/// </summary>
public class SingleBlockValue : BlockValue<SingleBlockLayoutItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockValue" /> class.
    /// </summary>
    public SingleBlockValue()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockValue" /> class.
    /// </summary>
    /// <param name="layout">The layout.</param>
    public SingleBlockValue(SingleBlockLayoutItem layout)
        => Layout[PropertyEditorAlias] = [layout];

    /// <inheritdoc />
    [JsonIgnore]
    public override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.SingleBlock;
}

using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Represents a rich text block value.
/// </summary>
public class RichTextBlockValue : BlockValue<RichTextBlockLayoutItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextBlockValue" /> class.
    /// </summary>
    public RichTextBlockValue()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextBlockValue" /> class.
    /// </summary>
    /// <param name="layouts">The layouts.</param>
    public RichTextBlockValue(IEnumerable<RichTextBlockLayoutItem> layouts)
        => Layout[PropertyEditorAlias] = layouts;

    /// <inheritdoc />
    [JsonIgnore]
    public override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.RichText;
}

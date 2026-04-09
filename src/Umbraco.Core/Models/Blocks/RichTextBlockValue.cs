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

    /// <inheritdoc />
#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable CS0618 // Type or member is obsolete
    public override bool SupportsBlockLayoutAlias(string alias) => base.SupportsBlockLayoutAlias(alias) || alias.Equals("Umbraco.TinyMCE");
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0672 // Member overrides obsolete member
}

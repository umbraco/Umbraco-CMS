using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core;

/// <summary>
/// Represents the content value stored in a rich text editor instance.
/// </summary>
[DataContract]
public class RichTextEditorValue
{
    /// <summary>
    /// Gets or sets the markup content of the rich text editor value.
    /// </summary>
    [DataMember(Name = "markup")]
    public required string Markup { get; set; }

    /// <summary>
    /// Gets or sets the collection of blocks representing the rich text content.
    /// </summary>
    [DataMember(Name = "blocks")]
    public RichTextBlockValue? Blocks { get; set; }
}

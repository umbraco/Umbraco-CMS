using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core;

[DataContract]
public class RichTextEditorValue
{
    [DataMember(Name = "markup")]
    public required string Markup { get; set; }

    [DataMember(Name = "blocks")]
    public required BlockValue? Blocks { get; set; }
}

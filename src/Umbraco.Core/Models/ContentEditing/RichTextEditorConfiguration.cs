using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "richtexteditorconfiguration", Namespace = "")]
public class RichTextEditorConfiguration
{
    [DataMember(Name = "plugins")]
    public IEnumerable<RichTextEditorPlugin>? Plugins { get; set; }

    [DataMember(Name = "commands")]
    public IEnumerable<RichTextEditorCommand>? Commands { get; set; }

    [DataMember(Name = "validElements")]
    public string? ValidElements { get; set; }

    [DataMember(Name = "inValidElements")]
    public string? InvalidElements { get; set; }

    [DataMember(Name = "customConfig")]
    public IDictionary<string, string>? CustomConfig { get; set; }
}

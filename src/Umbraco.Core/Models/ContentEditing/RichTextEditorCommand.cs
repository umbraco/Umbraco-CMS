using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "richtexteditorcommand", Namespace = "")]
public class RichTextEditorCommand
{
    [DataMember(Name = "name")] public string? Name { get; set; }

    [DataMember(Name = "alias")] public string? Alias { get; set; }

    [DataMember(Name = "mode")] public RichTextEditorCommandMode Mode { get; set; }
}

public enum RichTextEditorCommandMode
{
    Insert,
    Selection,
    All
}

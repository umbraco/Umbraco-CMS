using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "scriptFile", Namespace = "")]
public class SnippetDisplay
{
    [DataMember(Name = "name", IsRequired = true)]
    public string? Name { get; set; }

    [DataMember(Name = "fileName", IsRequired = true)]
    public string? FileName { get; set; }
}

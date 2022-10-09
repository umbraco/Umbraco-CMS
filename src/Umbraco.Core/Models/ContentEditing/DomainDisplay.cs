using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "DomainDisplay")]
public class DomainDisplay
{
    public DomainDisplay(string name, int lang)
    {
        Name = name;
        Lang = lang;
    }

    [DataMember(Name = "name")]
    public string Name { get; }

    [DataMember(Name = "lang")]
    public int Lang { get; }

    [DataMember(Name = "duplicate")]
    public bool Duplicate { get; set; }

    [DataMember(Name = "other")]
    public string? Other { get; set; }
}

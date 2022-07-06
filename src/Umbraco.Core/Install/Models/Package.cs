using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install.Models;

[Obsolete("This is no longer used")]
[DataContract(Name = "package")]
public class Package
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "thumbnail")]
    public string? Thumbnail { get; set; }

    [DataMember(Name = "id")]
    public Guid Id { get; set; }
}

using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "ContentDomainsAndCulture")]
public class ContentDomainsAndCulture
{
    [DataMember(Name = "domains")]
    public IEnumerable<DomainDisplay>? Domains { get; set; }

    [DataMember(Name = "language")]
    public string? Language { get; set; }
}

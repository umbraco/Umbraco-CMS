using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "contentRedirectUrl", Namespace = "")]
public class ContentRedirectUrl
{
    [DataMember(Name = "redirectId")]
    public Guid RedirectId { get; set; }

    [DataMember(Name = "originalUrl")]
    public string? OriginalUrl { get; set; }

    [DataMember(Name = "destinationUrl")]
    public string? DestinationUrl { get; set; }

    [DataMember(Name = "createDateUtc")]
    public DateTime CreateDateUtc { get; set; }

    [DataMember(Name = "contentId")]
    public int ContentId { get; set; }

    [DataMember(Name = "culture")]
    public string? Culture { get; set; }
}

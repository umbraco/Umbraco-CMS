using System.Net;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Core.Models;

[DataContract(Name = "upgrade", Namespace = "")]
public class UpgradeCheckResponse
{
    public UpgradeCheckResponse()
    {
    }

    public UpgradeCheckResponse(string upgradeType, string upgradeComment, string upgradeUrl, IUmbracoVersion umbracoVersion)
    {
        Type = upgradeType;
        Comment = upgradeComment;
        Url = upgradeUrl + "?version=" + WebUtility.UrlEncode(umbracoVersion.Version?.ToString(3));
    }

    [DataMember(Name = "type")]
    public string? Type { get; set; }

    [DataMember(Name = "comment")]
    public string? Comment { get; set; }

    [DataMember(Name = "url")]
    public string? Url { get; set; }
}

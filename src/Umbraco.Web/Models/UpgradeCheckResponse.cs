using System.Runtime.Serialization;
using System.Web;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Models
{
    [DataContract(Name = "upgrade", Namespace = "")]
    public class UpgradeCheckResponse
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        public UpgradeCheckResponse() { }
        public UpgradeCheckResponse(string upgradeType, string upgradeComment, string upgradeUrl)
        {
            Type = upgradeType;
            Comment = upgradeComment;
            Url = upgradeUrl + "?version=" + HttpUtility.UrlEncode(Current.UmbracoVersion.Current.ToString(3));
        }
    }
}

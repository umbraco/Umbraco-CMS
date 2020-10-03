using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Header
{
    [DataContract(Name = "badge", Namespace = "")]
    public class HeaderAppBadge
    {
        public HeaderAppBadge()
        {
            Type = HeaderAppBadgeType.Default;
        }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "type")]
        public HeaderAppBadgeType Type { get; set; }
    }
}

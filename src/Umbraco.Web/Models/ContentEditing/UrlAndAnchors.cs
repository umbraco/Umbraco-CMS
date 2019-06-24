using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "urlAndAnchors", Namespace = "")]
    public class UrlAndAnchors
    {
        public UrlAndAnchors(string url, IList<string> anchorValues)
        {
            Url = url;
            AnchorValues = anchorValues;
        }

        [DataMember(Name = "url")]
        public string Url { get; }

        [DataMember(Name = "anchorValues")]
        public IList<string> AnchorValues { get;  }
    }
}

using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "urlAndAnchors", Namespace = "")]
public class UrlAndAnchors
{
    public UrlAndAnchors(string url, IEnumerable<string> anchorValues)
    {
        Url = url;
        AnchorValues = anchorValues;
    }

    [DataMember(Name = "url")]
    public string Url { get; }

    [DataMember(Name = "anchorValues")]
    public IEnumerable<string> AnchorValues { get; }
}

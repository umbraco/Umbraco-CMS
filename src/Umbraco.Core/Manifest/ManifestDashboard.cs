using System.Runtime.Serialization;
using Umbraco.Cms.Core.Dashboards;

namespace Umbraco.Cms.Core.Manifest;

[DataContract]
public class ManifestDashboard : IDashboard
{
    [DataMember(Name = "weight")]
    public int Weight { get; set; } = 100;

    [DataMember(Name = "alias", IsRequired = true)]
    public string Alias { get; set; } = null!;

    [DataMember(Name = "view", IsRequired = true)]
    public string View { get; set; } = null!;

    [DataMember(Name = "sections")]
    public string[] Sections { get; set; } = Array.Empty<string>();

    [DataMember(Name = "access")]
    public IAccessRule[] AccessRules { get; set; } = Array.Empty<IAccessRule>();
}

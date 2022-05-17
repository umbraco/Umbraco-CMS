using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Dashboards;

[DataContract(IsReference = true)]
public class DashboardSlim : IDashboardSlim
{
    public string? Alias { get; set; }

    public string? View { get; set; }
}

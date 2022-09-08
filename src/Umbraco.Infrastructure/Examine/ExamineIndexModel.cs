using System.Runtime.Serialization;

namespace Umbraco.Cms.Infrastructure.Examine;

[DataContract(Name = "indexer", Namespace = "")]
public class ExamineIndexModel
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "healthStatus")]
    public string? HealthStatus { get; set; }

    [DataMember(Name = "isHealthy")]
    public bool IsHealthy => HealthStatus == "Healthy";

    [DataMember(Name = "providerProperties")]
    public IReadOnlyDictionary<string, object?>? ProviderProperties { get; set; }

    [DataMember(Name = "canRebuild")]
    public bool CanRebuild { get; set; }
}

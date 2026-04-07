using System.Runtime.Serialization;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Represents the data model used for an Examine index in Umbraco.
/// </summary>
[DataContract(Name = "indexer", Namespace = "")]
public class ExamineIndexModel
{
    /// <summary>
    /// Gets or sets the name of the index.
    /// </summary>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    /// The health status of the examine index.
    /// </summary>
    [DataMember(Name = "healthStatus")]
    public string? HealthStatus { get; set; }

    /// <summary>
    /// Gets a value indicating whether the index is considered healthy, meaning its <c>HealthStatus</c> property equals "Healthy".
    /// </summary>
    [DataMember(Name = "isHealthy")]
    public bool IsHealthy => HealthStatus == "Healthy";

    /// <summary>
    /// Gets or sets a collection of provider-specific properties for the examine index.
    /// The dictionary contains key-value pairs representing additional metadata or configuration for the index provider.
    /// </summary>
    [DataMember(Name = "providerProperties")]
    public IReadOnlyDictionary<string, object?>? ProviderProperties { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this index is allowed to be rebuilt.
    /// </summary>
    [DataMember(Name = "canRebuild")]
    public bool CanRebuild { get; set; }
}

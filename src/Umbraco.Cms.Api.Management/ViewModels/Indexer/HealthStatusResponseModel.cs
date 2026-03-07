namespace Umbraco.Cms.Api.Management.ViewModels.Indexer;

/// <summary>
/// Represents a response model containing the health status information for an indexer.
/// </summary>
public class HealthStatusResponseModel
{
    /// <summary>
    /// Gets the current health status of the indexer.
    /// </summary>
    public HealthStatus Status { get; init; }

    /// <summary>Gets or sets the message describing the health status.</summary>
    public string? Message { get; init; }
}

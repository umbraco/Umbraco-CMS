namespace Umbraco.Cms.Api.Management.ViewModels.Indexer;

/// <summary>
/// Defines the possible health statuses for an indexer in the system.
/// </summary>
public enum HealthStatus
{
    Healthy,
    Unhealthy,
    Rebuilding,
    Corrupt
}

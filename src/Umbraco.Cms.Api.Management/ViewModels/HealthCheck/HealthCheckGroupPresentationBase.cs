namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

/// <summary>
/// Serves as the base class for view models that present health check group information in the management API.
/// </summary>
public class HealthCheckGroupPresentationBase
{
    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    public required string Name { get; set; }
}

namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

/// <summary>
/// Represents the data model used to present a group of health checks in the API.
/// </summary>
public class HealthCheckGroupPresentationModel : HealthCheckGroupPresentationBase
{
    /// <summary>
    ///     Gets or sets the health checks.
    /// </summary>
    public required List<HealthCheckViewModel> Checks { get; set; }
}

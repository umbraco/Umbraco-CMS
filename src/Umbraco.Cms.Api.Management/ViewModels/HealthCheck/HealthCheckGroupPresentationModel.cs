namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckGroupPresentationModel : HealthCheckGroupPresentationBase
{
    /// <summary>
    ///     Gets or sets the health checks.
    /// </summary>
    public required List<HealthCheckViewModel> Checks { get; set; }
}

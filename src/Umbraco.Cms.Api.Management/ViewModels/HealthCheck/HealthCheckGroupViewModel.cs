namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckGroupViewModel : HealthCheckGroupModelBase
{
    /// <summary>
    ///     Gets or sets the health checks.
    /// </summary>
    public required List<HealthCheckViewModel> Checks { get; set; }
}

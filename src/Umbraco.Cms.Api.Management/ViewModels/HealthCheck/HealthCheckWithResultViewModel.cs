namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckWithResultViewModel : HealthCheckModelBase
{
    /// <summary>
    ///     Gets or sets the result(s) for a health check.
    ///     There can be several.
    /// </summary>
    public List<HealthCheckResultViewModel>? Results { get; set; }
}

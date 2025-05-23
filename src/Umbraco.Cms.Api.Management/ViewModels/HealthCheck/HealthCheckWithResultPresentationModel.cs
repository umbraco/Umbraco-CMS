namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckWithResultPresentationModel : HealthCheckModelBase
{
    /// <summary>
    ///     Gets or sets the result(s) for a health check.
    ///     There can be several.
    /// </summary>
    public List<HealthCheckResultResponseModel>? Results { get; set; }
}

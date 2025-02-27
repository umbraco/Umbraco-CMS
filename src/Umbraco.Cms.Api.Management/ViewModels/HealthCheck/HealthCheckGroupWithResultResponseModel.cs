namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckGroupWithResultResponseModel
{
    /// <summary>
    ///     Gets or sets the health checks with the result(s) from each health check.
    /// </summary>
    public required List<HealthCheckWithResultPresentationModel> Checks { get; set; }
}

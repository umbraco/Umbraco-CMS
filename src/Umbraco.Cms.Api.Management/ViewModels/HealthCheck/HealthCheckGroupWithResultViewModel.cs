namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckGroupWithResultViewModel
{
    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the health checks with the result(s) from each health check.
    /// </summary>
    public required List<HealthCheckWithResultViewModel> Checks { get; set; }
}

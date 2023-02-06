namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckGroupViewModel
{
    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the health checks.
    /// </summary>
    public required List<HealthCheckViewModel> Checks { get; set; }
}

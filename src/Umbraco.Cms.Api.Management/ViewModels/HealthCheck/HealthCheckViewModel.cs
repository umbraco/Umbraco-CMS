namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckViewModel
{
    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }
}

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Represents an acceptable configuration value for a health check.
/// </summary>
public class AcceptableConfiguration
{
    /// <summary>
    ///     Gets or sets the acceptable configuration value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this configuration value is the recommended setting.
    /// </summary>
    public bool IsRecommended { get; set; }
}

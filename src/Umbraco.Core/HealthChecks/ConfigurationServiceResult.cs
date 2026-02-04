namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Represents the result of a configuration service operation.
/// </summary>
public class ConfigurationServiceResult
{
    /// <summary>
    ///     Gets or sets a value indicating whether the configuration operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     Gets or sets the result message or value from the configuration operation.
    /// </summary>
    public string? Result { get; set; }
}

namespace Umbraco.Cms.Api.Management.ViewModels.HealthCheck;

public class HealthCheckActionRequestModel
{
    /// <summary>
    ///     Gets or sets the health check key.
    /// </summary>
    public required ReferenceByIdModel HealthCheck { get; set; }

    /// <summary>
    ///     Gets or sets the alias.
    /// </summary>
    /// <remarks>
    ///     It is used by the Health Check instance to execute the action.
    /// </remarks>
    public string? Alias { get; set; }

    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    /// <remarks>
    ///     It is used to name the "Fix" button.
    /// </remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether a value is required to rectify the issue.
    /// </summary>
    public required bool ValueRequired { get; set; }

    /// <summary>
    ///     Gets or sets the value to rectify the issue.
    /// </summary>
    public string? ProvidedValue { get; set; }

    /// <summary>
    ///     Gets or sets how the provided value is validated.
    /// </summary>
    public string? ProvidedValueValidation { get; set; }

    /// <summary>
    ///     Gets or sets the regex to use when validating the provided value (if the value can be validated by a regex).
    /// </summary>
    public string? ProvidedValueValidationRegex { get; set; }

    /// <summary>
    ///     Gets or sets the action parameters.
    /// </summary>
    public Dictionary<string, object>? ActionParameters { get; set; }
}

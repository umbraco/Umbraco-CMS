namespace Umbraco.Cms.Api.Management.ViewModels.Template;

/// <summary>
/// Represents a model used to return template configuration details in API responses.
/// </summary>
public class TemplateConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the template is disabled.
    /// </summary>
    public required bool Disabled { get; set; }
}

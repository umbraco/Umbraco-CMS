namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Model for validating update requests to an element.
/// </summary>
public class ValidateUpdateElementRequestModel : UpdateElementRequestModel
{
    /// <summary>
    /// Gets or sets the cultures to validate during the element update.
    /// </summary>
    public ISet<string>? Cultures { get; set; }
}

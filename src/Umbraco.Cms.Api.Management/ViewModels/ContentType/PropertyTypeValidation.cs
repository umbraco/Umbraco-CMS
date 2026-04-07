namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Defines the set of validation rules applied to a property type within a content type in Umbraco.
/// </summary>
public class PropertyTypeValidation
{
    /// <summary>
    /// Gets or sets a value indicating whether the property type is mandatory.
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    /// Gets or sets the message displayed when the property is mandatory.
    /// </summary>
    public string? MandatoryMessage { get; set; }

    /// <summary>
    /// Gets or sets the regular expression pattern that is used to validate the value of the property type.
    /// </summary>
    public string? RegEx { get; set; }

    /// <summary>Gets or sets the message to display when the regular expression validation fails.</summary>
    public string? RegExMessage { get; set; }
}

namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents the validation settings for a property type.
/// </summary>
public class PropertyTypeValidation
{
    /// <summary>
    ///     Gets or sets a value indicating whether the property value is mandatory.
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    ///     Gets or sets the custom validation message to display when a mandatory property is not filled in.
    /// </summary>
    public string? MandatoryMessage { get; set; }

    /// <summary>
    ///     Gets or sets the regular expression pattern used to validate the property value.
    /// </summary>
    public string? RegularExpression { get; set; }

    /// <summary>
    ///     Gets or sets the custom validation message to display when the regular expression validation fails.
    /// </summary>
    public string? RegularExpressionMessage { get; set; }
}

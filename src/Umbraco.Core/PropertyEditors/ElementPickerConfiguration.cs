namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the element picker property editor.
/// </summary>
public class ElementPickerConfiguration : IIgnoreUserStartNodesConfig
{
    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }

    /// <summary>
    /// Gets or sets the validation limits for the number of elements allowed.
    /// </summary>
    [ConfigurationField("validationLimit", Type = typeof(RangeConfigurationField))]
    public PropertyEditors.NumberRange? ValidationLimit { get; set; }

    /// <summary>
    /// Gets or sets the content type filter for allowed selections.
    /// </summary>
    [ConfigurationField("allowedContentTypes")]
    public string? AllowedContentTypeIds { get; set; }

    /// <summary>
    /// Represents a numeric range with optional minimum and maximum values.
    /// </summary>
    [Obsolete("No longer used by Umbraco; use Umbraco.Cms.Core.PropertyEditors.NumberRange instead. Scheduled for removal in Umbraco 21.")]
    public class NumberRange : PropertyEditors.NumberRange
    {
    }
}

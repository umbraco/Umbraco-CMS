namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the multiple document picker property editor.
/// </summary>
public class MultipleDocumentPickerConfiguration : IIgnoreUserStartNodesConfig
{
    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }

    /// <summary>
    /// Gets or sets the validation limits for the number of documents allowed.
    /// </summary>
    [ConfigurationField("validationLimit", Type = typeof(RangeConfigurationField))]
    public NumberRange? ValidationLimit { get; set; }

    /// <summary>
    /// Gets or sets the content type filter for allowed selections.
    /// </summary>
    [ConfigurationField("allowedContentTypes")]
    public string? AllowedContentTypeIds { get; set; }
}

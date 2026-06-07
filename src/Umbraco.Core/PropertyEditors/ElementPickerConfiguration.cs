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
    /// Gets or sets the validation limits for the number of blocks allowed.
    /// </summary>
    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new NumberRange();

    /// <summary>
    /// Gets or sets the content type filter for allowed selections.
    /// </summary>
    [ConfigurationField("allowedContentTypes")]
    public string? AllowedContentTypeIds { get; set; }

    /// <summary>
    /// Represents a numeric range with optional minimum and maximum values.
    /// </summary>
    public class NumberRange
    {
        /// <summary>
        /// Gets or sets the minimum value of the range.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of the range.
        /// </summary>
        public int? Max { get; set; }
    }
}

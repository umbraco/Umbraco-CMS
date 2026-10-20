namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the member picker property editor holding any number of members.
/// </summary>
public class MultipleMemberPickerConfiguration : MemberPickerConfigurationBase
{
    /// <summary>
    /// Gets or sets the validation limits for the number of members allowed.
    /// </summary>
    [ConfigurationField("validationLimit")]
    public NumberRange? ValidationLimit { get; set; }

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

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the member picker property editor holding any number of members.
/// </summary>
public class MultipleMemberPickerConfiguration : MemberPickerConfigurationBase
{
    /// <summary>
    /// Gets or sets the validation limits for the number of members allowed.
    /// </summary>
    [ConfigurationField("validationLimit", Type = typeof(RangeConfigurationField))]
    public NumberRange? ValidationLimit { get; set; }
}

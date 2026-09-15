namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the label value editor.
/// </summary>
[Obsolete("The label editors no longer take configuration: there is one editor per type of value a label can hold. Scheduled for removal in Umbraco 21.")]
#pragma warning disable CS0618 // Type or member is obsolete
public class LabelConfiguration : IConfigureValueType
#pragma warning restore CS0618 // Type or member is obsolete
{
    /// <summary>
    /// Gets or sets the value type for the label property editor.
    /// </summary>
    [ConfigurationField(Constants.PropertyEditors.ConfigurationKeys.DataValueType)]
    public string ValueType { get; set; } = ValueTypes.String;
}

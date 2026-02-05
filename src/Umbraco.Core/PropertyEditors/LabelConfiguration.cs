namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the label value editor.
/// </summary>
public class LabelConfiguration : IConfigureValueType
{
    /// <summary>
    /// Gets or sets the value type for the label property editor.
    /// </summary>
    [ConfigurationField(Constants.PropertyEditors.ConfigurationKeys.DataValueType)]
    public string ValueType { get; set; } = ValueTypes.String;
}

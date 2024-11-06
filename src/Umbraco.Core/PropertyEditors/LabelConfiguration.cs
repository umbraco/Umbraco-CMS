using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the label value editor.
/// </summary>
public class LabelConfiguration : IConfigureValueType
{
    [ConfigurationField(Constants.PropertyEditors.ConfigurationKeys.DataValueType)]
    [JsonPropertyName("umbracoDataValueType")]
    public string ValueType { get; set; } = ValueTypes.String;
}

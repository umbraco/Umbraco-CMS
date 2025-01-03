using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the true/false (toggle) value editor.
/// </summary>
public class TrueFalseConfiguration
{
    [ConfigurationField("default")]
    public bool InitialState { get; set; }
}

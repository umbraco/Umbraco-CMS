namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the textbox value editor.
/// </summary>
public class TextboxConfiguration
{
    [ConfigurationField("maxChars")]
    public int? MaxChars { get; set; }
}

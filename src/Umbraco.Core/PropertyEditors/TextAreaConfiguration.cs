namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the textarea value editor.
/// </summary>
public class TextAreaConfiguration
{
    [ConfigurationField("maxChars")]
    public int? MaxChars { get; set; }
}

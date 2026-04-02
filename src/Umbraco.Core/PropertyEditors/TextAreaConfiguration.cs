namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the textarea value editor.
/// </summary>
public class TextAreaConfiguration
{
    /// <summary>
    /// Gets or sets the maximum number of characters allowed.
    /// </summary>
    [ConfigurationField("maxChars")]
    public int? MaxChars { get; set; }
}

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the textbox value editor.
/// </summary>
public class TextboxConfiguration
{
    /// <summary>
    /// Gets or sets the maximum number of characters allowed.
    /// </summary>
    [ConfigurationField("maxChars")]
    public int? MaxChars { get; set; }
}

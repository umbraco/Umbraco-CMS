namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the textarea value editor.
/// </summary>
public class TextAreaConfiguration
{
    [ConfigurationField("maxChars", "Maximum allowed characters", "number", Description = "If empty - no character limit")]
    public int? MaxChars { get; set; }

    [ConfigurationField("rows", "Number of rows", "number", Description = "If empty - 10 rows would be set as the default value")]
    public int? Rows { get; set; }
}

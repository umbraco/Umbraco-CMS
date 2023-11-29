namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the Code Editor value editor.
/// </summary>
public class CodeEditorConfiguration
{
    [ConfigurationField("mode", "Mode", "textstring", Description = "")]
    public string? Mode { get; set; }

    [ConfigurationField("theme", "Theme", "textstring", Description = "")]
    public string? Theme { get; set; }
}

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the Code Editor value editor.
/// </summary>
public class CodeEditorConfiguration
{
    [ConfigurationField("mode", "Mode", "textstring", Description = "Select the programming language mode. The default mode is 'Razor'.")]
    public string? Mode { get; set; }

    [ConfigurationField("theme", "Theme", "textstring", Description = "Set the theme for the code editor. The default theme is 'Chrome'.")]
    public string? Theme { get; set; }

    [ConfigurationField("useWrapMode", "Word wrapping", "boolean", Description = "Select to enable word wrapping.")]
    public bool UseWrapMode { get; set; }

    [ConfigurationField("minLines", "Minimum lines", "number", Description = "Set the minimum number of lines that the editor will be. The default is 12 lines.")]
    public int MinLines { get; set; }

    [ConfigurationField("maxLines", "Maximum lines", "number", Description = "Set the maximum number of lines that the editor can be. If left empty, the editor will not auto-scale.")]
    public int MaxLines { get; set; }
}

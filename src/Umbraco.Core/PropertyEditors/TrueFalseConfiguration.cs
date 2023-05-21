namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the boolean value editor.
/// </summary>
public class TrueFalseConfiguration
{
    [ConfigurationField("default", "Initial State", "boolean", Description = "The initial state for the toggle, when it is displayed for the first time in the backoffice, eg. for a new content item.")]
    public bool Default { get; set; }

    [ConfigurationField("showLabels", "Show toggle labels", "boolean", Description = "Show labels next to toggle button.")]
    public bool ShowLabels { get; set; }

    [ConfigurationField("labelOn", "Label On", "textstring", Description = "Label text when enabled.")]
    public string? LabelOn { get; set; }

    [ConfigurationField("labelOff", "Label Off", "textstring", Description = "Label text when disabled.")]
    public string? LabelOff { get; set; }
}

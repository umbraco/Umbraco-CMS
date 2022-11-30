namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the rich text value editor.
/// </summary>
public class RichTextConfiguration : IIgnoreUserStartNodesConfig
{
    // TODO: Make these strongly typed, for now this works though
    [ConfigurationField("editor", "Editor", "views/propertyeditors/rte/rte.prevalues.html", HideLabel = true)]
    public object? Editor { get; set; }

    [ConfigurationField("overlaySize", "Overlay Size", "overlaysize", Description = "Select the width of the overlay (link picker).")]
    public string? OverlaySize { get; set; }

    [ConfigurationField("hideLabel", "Hide Label", "boolean")]
    public bool HideLabel { get; set; }

    [ConfigurationField("mediaParentId", "Image Upload Folder", "mediafolderpicker", Description = "Choose the upload location of pasted images")]
    public GuidUdi? MediaParentId { get; set; }

    [ConfigurationField(
        Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
        "Ignore User Start Nodes",
        "boolean",
        Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
    public bool IgnoreUserStartNodes { get; set; }
}

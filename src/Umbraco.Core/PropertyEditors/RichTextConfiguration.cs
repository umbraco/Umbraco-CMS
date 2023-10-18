namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the rich text value editor.
/// </summary>
public class RichTextConfiguration : IIgnoreUserStartNodesConfig
{
    // TODO: Make these strongly typed, for now this works though
    [ConfigurationField("editor", "Editor", "views/propertyeditors/rte/rte.prevalues.html", HideLabel = true)]
    public object? Editor { get; set; }

    // TODO: update descriptions + consider moving BlockListConfiguration.BlockConfiguration to its own class instead of being nested under BlockListConfiguration
    [ConfigurationField("blocks", "Available Blocks", "views/propertyeditors/rte/blocks/prevalue/blockrte.blockconfiguration.html", Description = "Define the available blocks.")]
    public BlockListConfiguration.BlockConfiguration[] Blocks { get; set; } = null!;

    [ConfigurationField("useLiveEditing", "Blocks Live editing mode", "boolean", Description = "Live updated Block Elements when they are edited.")]
    public bool UseLiveEditing { get; set; }

    [ConfigurationField("overlaySize", "Overlay Size", "overlaysize", Description = "Select the width of the link picker overlay.")]
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

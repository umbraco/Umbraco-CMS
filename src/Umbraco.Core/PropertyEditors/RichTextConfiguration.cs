using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the rich text value editor.
/// </summary>
public class RichTextConfiguration : IIgnoreUserStartNodesConfig
{
    // TODO: Make these strongly typed, for now this works though
    [ConfigurationField("editor", "Editor", "views/propertyeditors/rte/rte.prevalues.html", HideLabel = true)]
    public object? Editor { get; set; }

    [ConfigurationField("blocks", "Available Blocks", "views/propertyeditors/rte/blocks/prevalue/blockrte.blockconfiguration.html", Description = "Define the available blocks.")]
    public RichTextBlockConfiguration[]? Blocks { get; set; } = null!;

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

    [DataContract]
    public class RichTextBlockConfiguration : IBlockConfiguration
    {
        [DataMember(Name = "backgroundColor")]
        public string? BackgroundColor { get; set; }

        [DataMember(Name = "iconColor")]
        public string? IconColor { get; set; }

        [DataMember(Name = "thumbnail")]
        public string? Thumbnail { get; set; }

        [DataMember(Name = "contentElementTypeKey")]
        public Guid ContentElementTypeKey { get; set; }

        [DataMember(Name = "settingsElementTypeKey")]
        public Guid? SettingsElementTypeKey { get; set; }

        [DataMember(Name = "view")]
        public string? View { get; set; }

        [DataMember(Name = "stylesheet")]
        public string? Stylesheet { get; set; }

        [DataMember(Name = "label")]
        public string? Label { get; set; }

        [DataMember(Name = "editorSize")]
        public string? EditorSize { get; set; }

        [DataMember(Name = "forceHideContentEditorInOverlay")]
        public bool ForceHideContentEditorInOverlay { get; set; }

        [DataMember(Name = "displayInline")]
        public bool DisplayInline { get; set; }
    }
}

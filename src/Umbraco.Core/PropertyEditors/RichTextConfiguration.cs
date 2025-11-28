namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the rich text value editor.
/// </summary>
public class RichTextConfiguration : IIgnoreUserStartNodesConfig
{
    [ConfigurationField("blocks")]
    public RichTextBlockConfiguration[]? Blocks { get; set; } = Array.Empty<RichTextBlockConfiguration>();

    [ConfigurationField("mediaParentId")]
    public Guid? MediaParentId { get; set; }

    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }

    public class RichTextBlockConfiguration : IRichTextBlockConfiguration
    {
        public Guid ContentElementTypeKey { get; set; }

        public Guid? SettingsElementTypeKey { get; set; }

        public string? Label { get; set; }

        public string? EditorSize { get; set; }

        public string? IconColor { get; set; }

        public string? BackgroundColor { get; set; }

        public string? Thumbnail { get; set; }

        public bool? ForceHideContentEditorInOverlay { get; set; }

        public bool? DisplayInline { get; set; }
    }
}

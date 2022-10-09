using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The configuration object for the Block List editor
/// </summary>
public class BlockListConfiguration
{
    [ConfigurationField("blocks", "Available Blocks", "views/propertyeditors/blocklist/prevalue/blocklist.blockconfiguration.html", Description = "Define the available blocks.")]
    public BlockConfiguration[] Blocks { get; set; } = null!;

    [ConfigurationField("validationLimit", "Amount", "numberrange", Description = "Set a required range of blocks")]
    public NumberRange ValidationLimit { get; set; } = new();

    [ConfigurationField("useLiveEditing", "Live editing mode", "boolean", Description = "Live editing in editor overlays for live updated custom views or labels using custom expression.")]
    public bool UseLiveEditing { get; set; }

    [ConfigurationField("useInlineEditingAsDefault", "Inline editing mode", "boolean", Description = "Use the inline editor as the default block view.")]
    public bool UseInlineEditingAsDefault { get; set; }

    [ConfigurationField("maxPropertyWidth", "Property editor width", "textstring", Description = "Optional CSS override, example: 800px or 100%")]
    public string? MaxPropertyWidth { get; set; }

    [DataContract]
    public class BlockConfiguration
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
    }

    [DataContract]
    public class NumberRange
    {
        [DataMember(Name = "min")]
        public int? Min { get; set; }

        [DataMember(Name = "max")]
        public int? Max { get; set; }
    }
}

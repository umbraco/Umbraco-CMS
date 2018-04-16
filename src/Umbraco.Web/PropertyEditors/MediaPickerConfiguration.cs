using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the media picker value editor.
    /// </summary>
    public class MediaPickerConfiguration
    {
        [ConfigurationField("multiPicker", "Pick multiple items", "boolean")]
        public bool Multiple { get; set; }

        [ConfigurationField("onlyImages", "Pick only images", "boolean", Description = "Only let the editor choose images from media.")]
        public bool OnlyImages { get; set; }

        [ConfigurationField("disableFolderSelect", "Disable folder select", "boolean", Description = "Do not allow folders to be picked.")]
        public bool DisableFolderSelect { get; set; }

        [ConfigurationField("startNodeId", "Start node", "mediapicker")]
        public int StartNodeId { get; set; }
    }
}
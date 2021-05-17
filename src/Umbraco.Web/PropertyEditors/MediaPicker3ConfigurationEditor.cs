using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the media picker value editor.
    /// </summary>
    public class MediaPicker3ConfigurationEditor : ConfigurationEditor<MediaPicker3Configuration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPicker3ConfigurationEditor"/> class.
        /// </summary>
        public MediaPicker3ConfigurationEditor()
        {
            // configure fields
            // this is not part of ContentPickerConfiguration,
            // but is required to configure the UI editor (when editing the configuration)

            Field(nameof(MediaPicker3Configuration.StartNodeId))
                .Config = new Dictionary<string, object> { { "idType", "udi" } };

            Field(nameof(MediaPicker3Configuration.Filter))
                .Config = new Dictionary<string, object> { { "itemType", "media" } };
        }
    }
}

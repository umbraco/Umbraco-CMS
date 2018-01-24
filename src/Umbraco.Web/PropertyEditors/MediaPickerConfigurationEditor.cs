using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the media picker value editor.
    /// </summary>
    public class MediaPickerConfigurationEditor : ConfigurationEditor<MediaPickerConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerConfigurationEditor"/> class.
        /// </summary>
        public MediaPickerConfigurationEditor()
        {
            // must add that one explicitely due to field.Config
            Fields.Add(new ConfigurationField
            {
                Key = "startNodeId",
                View = "mediapicker",
                Name = "Start node",
                PropertyName = nameof(MediaPickerConfiguration.StartNodeId),
                Config = new Dictionary<string, object>
                {
                    {"idType", "udi"}
                }
            });
        }
    }
}
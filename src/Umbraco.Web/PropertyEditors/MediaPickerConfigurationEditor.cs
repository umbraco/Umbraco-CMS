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
            Field(nameof(MediaPickerConfiguration.StartNodeId))
                .Config = new Dictionary<string, object>
                {
                    { "idType", "udi" }
                };
        }

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var d = base.ToValueEditor(configuration);
            d["idType"] = "udi";
            return d;
        }
    }
}
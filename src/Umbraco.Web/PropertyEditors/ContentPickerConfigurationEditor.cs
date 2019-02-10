using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class ContentPickerConfigurationEditor : ConfigurationEditor<ContentPickerConfiguration>
    {
        public ContentPickerConfigurationEditor()
        {
            // configure fields
            // this is not part of ContentPickerConfiguration,
            // but is required to configure the UI editor (when editing the configuration)
            Field(nameof(ContentPickerConfiguration.StartNodeId))
                .Config = new Dictionary<string, object> { { "idType", "udi" } };
        }

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            // get the configuration fields
            var d = base.ToValueEditor(configuration);

            // add extra fields
            // not part of ContentPickerConfiguration but used to configure the UI editor
            d["showEditButton"] = false;
            d["showPathOnHover"] = false;
            d["idType"] = "udi"; 

            return d;
        }
    }
}

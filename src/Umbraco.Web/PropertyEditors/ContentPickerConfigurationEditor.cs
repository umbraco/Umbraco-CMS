using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class ContentPickerConfigurationEditor : ConfigurationEditor<ContentPickerConfiguration>
    {
        public ContentPickerConfigurationEditor()
        {
            // configure fields
            Field(nameof(ContentPickerConfiguration.StartNodeId))
                .Config = new Dictionary<string, object> { { "idType", "udi" } };
        }

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            // these are not configuration fields, but constants required by the value editor
            var d = base.ToValueEditor(configuration);
            d["showEditButton"] = false;
            d["showPathOnHover"] = false;
            d["idType"] = "udi";
            return d;
        }
    }
}

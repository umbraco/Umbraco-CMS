using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class MemberPickerConfigurationEditor : ConfigurationEditor<MemberPickerConfiguration>
    {
        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            // get the configuration fields
            var d = base.ToValueEditor(configuration);

            // add extra fields
            // not part of MemberPickerConfiguration but used to configure the UI editor
            d["idType"] = "udi";

            return d;
        }
    }
}

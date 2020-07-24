using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class MemberPickerConfigurationEditor : ConfigurationEditor<MemberPickerConfiguration>
    {
        /// <inheritdoc />
        public override Dictionary<string, object> ToConfigurationEditor(MemberPickerConfiguration configuration)
        {
            // sanitize configuration
            var output = base.ToConfigurationEditor(configuration);

            output["multiPicker"] = configuration.MaxNumber > 1;

            return output;
        }

        /// <inheritdoc />
        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            // get the configuration fields
            var d = base.ToValueEditor(configuration);

            d["multiPicker"] = false;
            d["showEditButton"] = false;

            // add extra fields
            // not part of MemberPickerConfiguration but used to configure the UI editor
            d["idType"] = "udi";

            return d;
        }
    }
}

using System.Collections.Generic;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the multinode picker value editor.
    /// </summary>
    public class MultiNodePickerConfigurationEditor : ConfigurationEditor<MultiNodePickerConfiguration>
    {
        public MultiNodePickerConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
            Field(nameof(MultiNodePickerConfiguration.TreeSource))
                .Config = new Dictionary<string, object> { { "idType", "udi" } };
        }

        /// <inheritdoc />
        public override Dictionary<string, object> ToConfigurationEditor(MultiNodePickerConfiguration configuration)
        {
            // sanitize configuration
            var output = base.ToConfigurationEditor(configuration);

            output["multiPicker"] = configuration.MaxNumber > 1;

            return output;
        }

        /// <inheritdoc />
        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var d = base.ToValueEditor(configuration);
            d["multiPicker"] = true;
            d["showEditButton"] = false;
            d["showPathOnHover"] = false;
            d["idType"] = "udi";
            return d;
        }
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors
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
        public override Dictionary<string, object> ToConfigurationEditor(MultiNodePickerConfiguration? configuration)
        {
            // sanitize configuration
            var output = base.ToConfigurationEditor(configuration);

            output["multiPicker"] = configuration?.MaxNumber > 1;

            return output;
        }

        /// <inheritdoc />
        public override IDictionary<string, object> ToValueEditor(object? configuration)
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

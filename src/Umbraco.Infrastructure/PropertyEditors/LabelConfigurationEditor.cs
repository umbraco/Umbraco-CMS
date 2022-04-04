// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the label value editor.
    /// </summary>
    public class LabelConfigurationEditor : ConfigurationEditor<LabelConfiguration>
    {
        public LabelConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }

        /// <inheritdoc />
        public override LabelConfiguration FromConfigurationEditor(IDictionary<string, object?>? editorValues, LabelConfiguration? configuration)
        {
            var newConfiguration = new LabelConfiguration();

            // get the value type
            // not simply deserializing Json because we want to validate the valueType

            if (editorValues is not null && editorValues.TryGetValue(Cms.Core.Constants.PropertyEditors.ConfigurationKeys.DataValueType, out var valueTypeObj)
                && valueTypeObj is string stringValue)
            {
                if (!string.IsNullOrWhiteSpace(stringValue) && ValueTypes.IsValue(stringValue)) // validate
                    newConfiguration.ValueType = stringValue;
            }

            return newConfiguration;
        }


    }
}

using System.Collections.Generic;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the label value editor.
    /// </summary>
    public class LabelConfigurationEditor : ConfigurationEditor<LabelConfiguration>
    {
        /// <inheritdoc />
        public override LabelConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, LabelConfiguration configuration)
        {
            var newConfiguration = new LabelConfiguration();

            // get the value type
            // not simply deserializing Json because we want to validate the valueType

            if (editorValues.TryGetValue(Constants.PropertyEditors.ConfigurationKeys.DataValueType, out var valueTypeObj)
                && valueTypeObj is string stringValue)
            {
                if (!string.IsNullOrWhiteSpace(stringValue) && ValueTypes.IsValue(stringValue)) // validate
                    newConfiguration.ValueType = stringValue;
            }

            return newConfiguration;
        }
    }
}
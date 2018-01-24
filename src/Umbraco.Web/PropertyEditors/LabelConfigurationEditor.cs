using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the label value editor.
    /// </summary>
    public class LabelConfigurationEditor : ConfigurationEditor<LabelConfiguration>
    {
        /// <inheritdoc />
        public override LabelConfiguration FromEditor(Dictionary<string, object> editorValue, LabelConfiguration configuration)
        {
            var newConfiguration = new LabelConfiguration();

            // get the value type
            // not simply deserializing Json because we want to validate the valueType

            if (editorValue.TryGetValue(Constants.PropertyEditors.ConfigurationKeys.DataValueType, out var valueTypeObj)
                && valueTypeObj is JToken jtoken
                && jtoken.Type == JTokenType.String)
            {
                var valueType = jtoken.Value<string>();
                if (!string.IsNullOrWhiteSpace(valueType) && ValueTypes.IsValue(valueType)) // validate
                    newConfiguration.ValueType = valueType;
            }

            return newConfiguration;
        }
    }
}
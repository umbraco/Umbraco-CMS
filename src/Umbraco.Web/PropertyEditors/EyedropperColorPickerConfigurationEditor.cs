using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class EyedropperColorPickerConfigurationEditor : ConfigurationEditor<EyedropperColorPickerConfiguration>
    {
        public EyedropperColorPickerConfigurationEditor()
        {

        }

        public override Dictionary<string, object> ToConfigurationEditor(EyedropperColorPickerConfiguration configuration)
        {
            var showAlpha = configuration?.ShowAlpha ?? false;

            return new Dictionary<string, object>
            {
                { "showAlpha", showAlpha }
            };
        }

        public override EyedropperColorPickerConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, EyedropperColorPickerConfiguration configuration)
        {
            var output = new EyedropperColorPickerConfiguration();

            if (editorValues.TryGetValue("showAlpha", out var alpha))
            {
                var attempt = alpha.TryConvertTo<bool>();
                if (attempt.Success)
                    output.ShowAlpha = attempt.Result;
            }

            return output;
        }
    }
}

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

        /// <inheritdoc />
        public override Dictionary<string, object> ToConfigurationEditor(EyedropperColorPickerConfiguration configuration)
        {
            return new Dictionary<string, object>
            {
                { "showAlpha", configuration.ShowAlpha },
                { "showPalette", configuration.ShowPalette }
            };
        }

        /// <inheritdoc />
        public override EyedropperColorPickerConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, EyedropperColorPickerConfiguration configuration)
        {
            var output = new EyedropperColorPickerConfiguration();

            if (editorValues.TryGetValue("showAlpha", out var alpha))
            {
                var attempt = alpha.TryConvertTo<bool>();
                if (attempt.Success)
                    output.ShowAlpha = attempt.Result;
            }

            if (editorValues.TryGetValue("showPalette", out var palette))
            {
                var attempt = palette.TryConvertTo<bool>();
                if (attempt.Success)
                    output.ShowPalette = attempt.Result;
            }

            return output;
        }
    }
}

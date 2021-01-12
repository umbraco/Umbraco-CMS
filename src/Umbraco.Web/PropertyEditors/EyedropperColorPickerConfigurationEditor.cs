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

            var showAlpha = true;
            var showPalette = true;

            if (editorValues.TryGetValue("showAlpha", out var alpha))
            {
                var attempt = alpha.TryConvertTo<bool>();
                if (attempt.Success)
                    showAlpha = attempt.Result;
            }

            if (editorValues.TryGetValue("showPalette", out var palette))
            {
                var attempt = palette.TryConvertTo<bool>();
                if (attempt.Success)
                    showPalette = attempt.Result;
            }

            return new EyedropperColorPickerConfiguration
            {
                ShowAlpha = showAlpha,
                ShowPalette = showPalette
            };
        }
    }
}

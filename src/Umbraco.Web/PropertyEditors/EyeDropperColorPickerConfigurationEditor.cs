using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class EyeDropperColorPickerConfigurationEditor : ConfigurationEditor<EyeDropperColorPickerConfiguration>
    {
        public EyeDropperColorPickerConfigurationEditor()
        {

        }

        /// <inheritdoc />
        public override Dictionary<string, object> ToConfigurationEditor(EyeDropperColorPickerConfiguration configuration)
        {
            return new Dictionary<string, object>
            {
                { "showAlpha", configuration.ShowAlpha },
                { "showPalette", configuration.ShowPalette }
            };
        }

        /// <inheritdoc />
        public override EyeDropperColorPickerConfiguration FromConfigurationEditor(IDictionary<string, object> editorValues, EyeDropperColorPickerConfiguration configuration)
        {
            var output = new EyeDropperColorPickerConfiguration();

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

            return new EyeDropperColorPickerConfiguration
            {
                ShowAlpha = showAlpha,
                ShowPalette = showPalette
            };
        }
    }
}

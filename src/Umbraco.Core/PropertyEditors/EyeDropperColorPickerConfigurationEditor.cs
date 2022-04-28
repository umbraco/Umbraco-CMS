
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    internal class EyeDropperColorPickerConfigurationEditor : ConfigurationEditor<EyeDropperColorPickerConfiguration>
    {
        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public EyeDropperColorPickerConfigurationEditor(IIOHelper ioHelper)
            : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        public EyeDropperColorPickerConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser) : base(ioHelper, editorConfigurationParser)
        {
        }

        /// <inheritdoc />
        public override Dictionary<string, object> ToConfigurationEditor(EyeDropperColorPickerConfiguration? configuration)
        {
            return new Dictionary<string, object>
            {
                { "showAlpha", configuration?.ShowAlpha ?? false },
                { "showPalette", configuration?.ShowPalette ?? false },
            };
        }

        /// <inheritdoc />
        public override EyeDropperColorPickerConfiguration FromConfigurationEditor(IDictionary<string, object?>? editorValues, EyeDropperColorPickerConfiguration? configuration)
        {
            var showAlpha = true;
            var showPalette = true;

            if (editorValues is not null && editorValues.TryGetValue("showAlpha", out var alpha))
            {
                var attempt = alpha.TryConvertTo<bool>();
                if (attempt.Success)
                    showAlpha = attempt.Result;
            }

            if (editorValues is not null && editorValues.TryGetValue("showPalette", out var palette))
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

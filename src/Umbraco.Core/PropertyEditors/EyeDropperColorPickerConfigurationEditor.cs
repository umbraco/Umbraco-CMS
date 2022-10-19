using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class EyeDropperColorPickerConfigurationEditor : ConfigurationEditor<EyeDropperColorPickerConfiguration>
{
    public EyeDropperColorPickerConfigurationEditor(
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
    }

    /// <inheritdoc />
    public override Dictionary<string, object> ToConfigurationEditor(EyeDropperColorPickerConfiguration? configuration) =>
        new()
        {
            { "showAlpha", configuration?.ShowAlpha ?? false }, { "showPalette", configuration?.ShowPalette ?? false },
        };

    /// <inheritdoc />
    public override EyeDropperColorPickerConfiguration FromConfigurationEditor(
        IDictionary<string, object?>? editorValues, EyeDropperColorPickerConfiguration? configuration)
    {
        var showAlpha = true;
        var showPalette = true;

        if (editorValues is not null && editorValues.TryGetValue("showAlpha", out var alpha))
        {
            Attempt<bool> attempt = alpha.TryConvertTo<bool>();
            if (attempt.Success)
            {
                showAlpha = attempt.Result;
            }
        }

        if (editorValues is not null && editorValues.TryGetValue("showPalette", out var palette))
        {
            Attempt<bool> attempt = palette.TryConvertTo<bool>();
            if (attempt.Success)
            {
                showPalette = attempt.Result;
            }
        }

        return new EyeDropperColorPickerConfiguration { ShowAlpha = showAlpha, ShowPalette = showPalette };
    }
}

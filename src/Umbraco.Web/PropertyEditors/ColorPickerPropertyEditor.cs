using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.ColorPicker,
        "Color Picker",
        "colorpicker",
        Icon = "icon-colorpicker",
        Group = Constants.PropertyEditors.Groups.Pickers)]
    public class ColorPickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        public ColorPickerPropertyEditor(ILogger logger, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper, IShortStringHelper shortStringHelper)
            : base(logger, dataTypeService, localizationService, shortStringHelper)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ColorPickerConfigurationEditor(_ioHelper);
    }
}

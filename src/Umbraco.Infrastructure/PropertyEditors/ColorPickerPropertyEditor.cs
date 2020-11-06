using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.IO;
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

        public ColorPickerPropertyEditor(ILoggerFactory loggerFactory, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper, IShortStringHelper shortStringHelper, ILocalizedTextService localizedTextService)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ColorPickerConfigurationEditor(_ioHelper);
    }
}

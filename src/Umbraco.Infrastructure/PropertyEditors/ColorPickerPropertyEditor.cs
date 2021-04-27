// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
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

        public ColorPickerPropertyEditor(ILoggerFactory loggerFactory, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper, IShortStringHelper shortStringHelper, ILocalizedTextService localizedTextService, IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ColorPickerConfigurationEditor(_ioHelper, JsonSerializer);
    }
}

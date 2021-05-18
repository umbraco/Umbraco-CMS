// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

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
        private readonly IJsonSerializer _jsonSerializer;

        public ColorPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            IJsonSerializer jsonSerializer)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
            _jsonSerializer = jsonSerializer;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ColorPickerConfigurationEditor(_ioHelper, _jsonSerializer);
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

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
        private readonly IEditorConfigurationParser _editorConfigurationParser;

        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public ColorPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            IJsonSerializer jsonSerializer)
            : this(dataValueEditorFactory, ioHelper, jsonSerializer, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        public ColorPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            IJsonSerializer jsonSerializer,
            IEditorConfigurationParser editorConfigurationParser)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
            _jsonSerializer = jsonSerializer;
            _editorConfigurationParser = editorConfigurationParser;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ColorPickerConfigurationEditor(_ioHelper, _jsonSerializer, _editorConfigurationParser);
    }
}

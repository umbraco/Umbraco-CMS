﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.ColorPickerEyeDropper,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Eye Dropper Color Picker",
        "eyedropper",
        Icon = "icon-colorpicker",
        Group = Constants.PropertyEditors.Groups.Pickers)]
    public class EyeDropperColorPickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;
        private readonly IEditorConfigurationParser _editorConfigurationParser;

        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public EyeDropperColorPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            EditorType type = EditorType.PropertyValue)
            : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>(), type)
        {
        }

        public EyeDropperColorPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            IEditorConfigurationParser editorConfigurationParser,
            EditorType type = EditorType.PropertyValue)
            : base(dataValueEditorFactory, type)
        {
            _ioHelper = ioHelper;
            _editorConfigurationParser = editorConfigurationParser;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new EyeDropperColorPickerConfigurationEditor(_ioHelper, _editorConfigurationParser);
    }
}

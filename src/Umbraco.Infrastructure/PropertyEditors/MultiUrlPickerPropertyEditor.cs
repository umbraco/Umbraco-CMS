// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultiUrlPicker,
        EditorType.PropertyValue,
        "Multi URL Picker",
        "multiurlpicker",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Pickers,
        Icon = "icon-link")]
    public class MultiUrlPickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;
        private readonly IEditorConfigurationParser _editorConfigurationParser;

        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public MultiUrlPickerPropertyEditor(
            IIOHelper ioHelper,
            IDataValueEditorFactory dataValueEditorFactory)
            : this(ioHelper, dataValueEditorFactory, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        public MultiUrlPickerPropertyEditor(
            IIOHelper ioHelper,
            IDataValueEditorFactory dataValueEditorFactory,
            IEditorConfigurationParser editorConfigurationParser)
            : base(dataValueEditorFactory, EditorType.PropertyValue)
        {
            _ioHelper = ioHelper;
            _editorConfigurationParser = editorConfigurationParser;
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiUrlPickerConfigurationEditor(_ioHelper, _editorConfigurationParser);

        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<MultiUrlPickerValueEditor>(Attribute!);
    }
}

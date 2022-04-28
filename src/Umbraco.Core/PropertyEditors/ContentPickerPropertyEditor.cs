// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Content property editor that stores UDI
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.ContentPicker,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Content Picker",
        "contentpicker",
        ValueType = ValueTypes.String,
        Group = Constants.PropertyEditors.Groups.Pickers)]
    public class ContentPickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;
        private readonly IEditorConfigurationParser _editorConfigurationParser;

        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public ContentPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper)
            : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        public ContentPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            IEditorConfigurationParser editorConfigurationParser)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
            _editorConfigurationParser = editorConfigurationParser;
        }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor(_ioHelper);
        }

        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<ContentPickerPropertyValueEditor>(Attribute!);

        internal class ContentPickerPropertyValueEditor  : DataValueEditor, IDataValueReference
        {
            public ContentPickerPropertyValueEditor(
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                IIOHelper ioHelper,
                DataEditorAttribute attribute)
                : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
            {
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
            {
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString)) yield break;

                if (UdiParser.TryParse(asString, out var udi))
                    yield return new UmbracoEntityReference(udi);
            }
        }
    }
}

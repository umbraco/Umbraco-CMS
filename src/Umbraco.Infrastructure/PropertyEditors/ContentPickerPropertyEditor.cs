﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

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
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IIOHelper _ioHelper;

        public ContentPickerPropertyEditor(
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            ILoggerFactory loggerFactory,
            IIOHelper ioHelper,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService,localizationService,localizedTextService, shortStringHelper, jsonSerializer)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _localizedTextService = localizedTextService;
            _ioHelper = ioHelper;
        }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor(_ioHelper);
        }

        protected override IDataValueEditor CreateValueEditor() => new ContentPickerPropertyValueEditor(_dataTypeService, _localizationService, _localizedTextService, ShortStringHelper, JsonSerializer, Attribute);

        internal class ContentPickerPropertyValueEditor  : DataValueEditor, IDataValueReference
        {
            public ContentPickerPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, ILocalizedTextService localizedTextService, IShortStringHelper shortStringHelper, IJsonSerializer jsonSerializer, DataEditorAttribute attribute)
                : base(dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer, attribute)
            {
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString)) yield break;

                if (UdiParser.TryParse(asString, out var udi))
                    yield return new UmbracoEntityReference(udi);
            }
        }
    }
}

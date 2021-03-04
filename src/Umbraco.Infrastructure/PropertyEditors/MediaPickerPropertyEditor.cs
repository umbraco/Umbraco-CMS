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
    /// Represents a media picker property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MediaPicker,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Media Picker",
        "mediapicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = Constants.Icons.MediaImage)]
    public class MediaPickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerPropertyEditor"/> class.
        /// </summary>
        public MediaPickerPropertyEditor(
            ILoggerFactory loggerFactory,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            IIOHelper ioHelper,
            IShortStringHelper shortStringHelper,
            ILocalizedTextService localizedTextService,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPickerConfigurationEditor(_ioHelper);

        protected override IDataValueEditor CreateValueEditor() => new MediaPickerPropertyValueEditor(DataTypeService, LocalizationService, LocalizedTextService, ShortStringHelper, JsonSerializer, Attribute);

        public class MediaPickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MediaPickerPropertyValueEditor(
                IDataTypeService dataTypeService,
                ILocalizationService localizationService,
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                DataEditorAttribute attribute)
                : base(dataTypeService,localizationService, localizedTextService, shortStringHelper, jsonSerializer, attribute)
            {
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString)) yield break;

                foreach (var udiStr in asString.Split(','))
                {
                    if (UdiParser.TryParse(udiStr, out var udi))
                        yield return new UmbracoEntityReference(udi);
                }
            }
        }
    }
}

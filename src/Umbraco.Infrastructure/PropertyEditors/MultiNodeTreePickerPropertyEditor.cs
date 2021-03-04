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
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
        "Multinode Treepicker",
        "contentpicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Pickers,
        Icon = "icon-page-add")]
    public class MultiNodeTreePickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        public MultiNodeTreePickerPropertyEditor(
            ILoggerFactory loggerFactory,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IIOHelper ioHelper,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _ioHelper = ioHelper;
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiNodePickerConfigurationEditor(_ioHelper);

        protected override IDataValueEditor CreateValueEditor() => new MultiNodeTreePickerPropertyValueEditor(DataTypeService, LocalizationService, LocalizedTextService, ShortStringHelper, JsonSerializer, Attribute);

        public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MultiNodeTreePickerPropertyValueEditor(
                IDataTypeService dataTypeService,
                ILocalizationService localizationService,
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                DataEditorAttribute attribute)
                : base(dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer, attribute)
            {

            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

                var udiPaths = asString.Split(',');
                foreach (var udiPath in udiPaths)
                    if (UdiParser.TryParse(udiPath, out var udi))
                        yield return new UmbracoEntityReference(udi);
            }
        }
    }


}

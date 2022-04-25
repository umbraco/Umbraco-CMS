// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
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
    /// <remarks>
    /// Named "(legacy)" as it's best to use the NEW Media Picker aka MediaPicker3
    /// </remarks>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MediaPicker,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Media Picker (legacy)",
        "mediapicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = Constants.Icons.MediaImage,
        IsDeprecated = false)]
    public class MediaPickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerPropertyEditor"/> class.
        /// </summary>
        public MediaPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPickerConfigurationEditor(_ioHelper);

        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<MediaPickerPropertyValueEditor>(Attribute!);

        public class MediaPickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MediaPickerPropertyValueEditor(
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

                foreach (var udiStr in asString.Split(','))
                {
                    if (UdiParser.TryParse(udiStr, out var udi))
                        yield return new UmbracoEntityReference(udi);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors
{
    /// <summary>
    /// Represents a multiple media picker macro parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultipleMediaPicker,
        EditorType.MacroParameter,
        "Multiple Media Picker",
        "mediapicker",
        ValueType = ValueTypes.Text)]
    public class MultipleMediaPickerParameterEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleMediaPickerParameterEditor"/> class.
        /// </summary>
        public MultipleMediaPickerParameterEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
            DefaultConfiguration.Add("multiPicker", "1");
        }

        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<MultipleMediaPickerPropertyValueEditor>(Attribute);

        internal class MultipleMediaPickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly IEntityService _entityService;

            public MultipleMediaPickerPropertyValueEditor(
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                IIOHelper ioHelper,
                DataEditorAttribute attribute,
                IEntityService entityService)
                : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
            {
                _entityService = entityService;
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString))
                {
                    yield break;
                }

                foreach (var udiStr in asString.Split(','))
                {
                    if (UdiParser.TryParse(udiStr, out Udi udi))
                    {
                        yield return new UmbracoEntityReference(udi);
                    }

                    // this is needed to support the legacy case when the multiple media picker parameter editor stores ints not udis
                    if (int.TryParse(udiStr, out var id))
                    {
                        Attempt<Guid> guidAttempt = _entityService.GetKey(id, UmbracoObjectTypes.Media);
                        Guid guid = guidAttempt.Success ? guidAttempt.Result : Guid.Empty;

                        if (guid != Guid.Empty)
                        {
                            yield return new UmbracoEntityReference(new GuidUdi(Constants.UdiEntityType.Media, guid));
                        }

                    }
                }
            }
        }
    }
}

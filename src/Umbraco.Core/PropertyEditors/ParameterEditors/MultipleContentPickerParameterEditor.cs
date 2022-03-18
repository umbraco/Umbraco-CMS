using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors
{
    /// <summary>
    /// Represents a parameter editor of some sort.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
        EditorType.MacroParameter,
        "Multiple Content Picker",
        "contentpicker")]
    public class MultipleContentPickerParameterEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleContentPickerParameterEditor"/> class.
        /// </summary>
        public MultipleContentPickerParameterEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
            // configure
            DefaultConfiguration.Add("multiPicker", "1");
            DefaultConfiguration.Add("minNumber",0 );
            DefaultConfiguration.Add("maxNumber", 0);
        }

        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<MultipleContentPickerParameterEditor.MultipleContentPickerParamateterValueEditor>(Attribute);

        internal class MultipleContentPickerParamateterValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly IEntityService _entityService;

            public MultipleContentPickerParamateterValueEditor(
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
                        Attempt<Guid> guidAttempt = _entityService.GetKey(id, UmbracoObjectTypes.Document);
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

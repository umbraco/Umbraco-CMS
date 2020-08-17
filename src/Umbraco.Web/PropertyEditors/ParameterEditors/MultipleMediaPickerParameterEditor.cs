using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using static Umbraco.Web.PropertyEditors.MediaPickerPropertyEditor;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
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
        public MultipleMediaPickerParameterEditor(ILogger logger)
            : base(logger)
        {
            DefaultConfiguration.Add("multiPicker", "1");
        }
        protected override IDataValueEditor CreateValueEditor() => new MultipleMediaPickerPropertyValueEditor(Attribute);

        internal class MultipleMediaPickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MultipleMediaPickerPropertyValueEditor(DataEditorAttribute attribute) : base(attribute)
            {
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString)) yield break;

                foreach (var udiStr in asString.Split(','))
                {
                    if (Udi.TryParse(udiStr, out var udi))
                    {
                        yield return new UmbracoEntityReference(udi);
                    }
                    // for legacy reasons the multiple media picker parameter editor is configured to store as ints not udis - there is a PR to perhaps change this: https://github.com/umbraco/Umbraco-CMS/pull/8388
                    // but adding below should support both scenarios, or should this be added as a fallback on the MediaPickerPropertyValueEditor
                    if (int.TryParse(udiStr, out var id))
                    {
                        //TODO: inject the service?
                        var guidAttempt = Current.Services.EntityService.GetKey(id, UmbracoObjectTypes.Media);
                        var guid = guidAttempt.Success ? guidAttempt.Result : Guid.Empty;
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

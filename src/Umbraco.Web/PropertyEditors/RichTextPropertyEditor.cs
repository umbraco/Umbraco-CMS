using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Macros;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TinyMCEAlias, "Rich Text Editor", "rte", ValueType = "TEXT",  HideLabel = false, Group="Rich Content", Icon="icon-browser-window")]
    public class RichTextPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Create a custom value editor
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new RichTextPropertyValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new RichTextPreValueEditor();
        }


        /// <summary>
        /// A custom value editor to ensure that macro syntax is parsed when being persisted and formatted correctly for display in the editor
        /// </summary>
        internal class RichTextPropertyValueEditor : PropertyValueEditorWrapper
        {
            public RichTextPropertyValueEditor(PropertyValueEditor wrapped)
                : base(wrapped)
            {
            }

            /// <summary>
            /// override so that we can hide the label based on the pre-value
            /// </summary>
            /// <param name="preValues"></param>
            public override void ConfigureForDisplay(Core.Models.PreValueCollection preValues)
            {
                base.ConfigureForDisplay(preValues);
                var asDictionary = preValues.FormatAsDictionary();
                if (asDictionary.ContainsKey("hideLabel"))
                {
                    var boolAttempt = asDictionary["hideLabel"].Value.TryConvertTo<bool>();
                    if (boolAttempt.Success)
                    {
                        HideLabel = boolAttempt.Result;
                    }
                }
            }

            /// <summary>
            /// Format the data for the editor
            /// </summary>
            /// <param name="property"></param>
            /// <param name="propertyType"></param>
            /// <param name="dataTypeService"></param>
            /// <returns></returns>
            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                if (property.Value == null)
                    return null;

                var parsed = MacroTagParser.FormatRichTextPersistedDataForEditor(property.Value.ToString(), new Dictionary<string, string>());
                return parsed;
            }

            /// <summary>
            /// Format the data for persistence
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object ConvertEditorToDb(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null)
                    return null;

                var parsed = MacroTagParser.FormatRichTextContentForPersistence(editorValue.Value.ToString());
                return parsed;
            }
        }
    }

    
}
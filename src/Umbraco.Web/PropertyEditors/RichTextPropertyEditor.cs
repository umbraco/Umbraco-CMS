using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Macros;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TinyMCEv3Alias, "Rich Text Editor", "rte")]
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
            /// Format the data for the editor
            /// </summary>
            /// <param name="dbValue"></param>
            /// <returns></returns>
            public override object FormatDataForEditor(object dbValue)
            {
                if (dbValue == null)
                    return dbValue;

                var parsed = MacroTagParser.FormatRichTextPersistedDataForEditor(dbValue.ToString(), new Dictionary<string, string>());
                return parsed;
            }

            /// <summary>
            /// Format the data for persistence
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object FormatDataForPersistence(Core.Models.Editors.ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null)
                    return null;

                var parsed = MacroTagParser.FormatRichTextContentForPersistence(editorValue.Value.ToString());
                return parsed;
            }
        }
    }

    
}
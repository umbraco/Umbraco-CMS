using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.NoEditAlias, "Label", "readonlyvalue", Icon="icon-readonly")]
    public class LabelPropertyEditor : PropertyEditor
    {

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new LabelPropertyValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new LabelPreValueEditor();
        }
        
        /// <summary>
        /// Custom value editor to mark it as readonly
        /// </summary>
        internal class LabelPropertyValueEditor : PropertyValueEditorWrapper
        {
            public LabelPropertyValueEditor(PropertyValueEditor wrapped)
                : base(wrapped)
            {
            }

            /// <summary>
            /// This editor is for display purposes only, any values bound to it will not be saved back to the database
            /// </summary>
            public override bool IsReadOnly
            {
                get { return true; }
            }
        }

        internal class LabelPreValueEditor : PreValueEditor
        {
            public LabelPreValueEditor()
            {
                Fields.Add(new PreValueField()
                {
                    HideLabel = true,
                    View = "readonlykeyvalues",
                    Key = "values"
                });
            }

            /// <summary>
            /// Chuck all the values into one field so devs can see what is stored there - we want this in case we've converted a legacy proeprty editor over to a label
            /// we should still show the pre-values stored for the data type.
            /// </summary>
            /// <param name="defaultPreVals"></param>
            /// <param name="persistedPreVals"></param>
            /// <returns></returns>
            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var existing = base.ConvertDbToEditor(defaultPreVals, persistedPreVals);
                //convert to a list, easier to enumerate on the editor
                var asList = existing.Select(e => new KeyValuePair<string, object>(e.Key, e.Value)).ToList();
                var result = new Dictionary<string, object> { { "values", asList } };
                return result;
            }
        }

    }
}
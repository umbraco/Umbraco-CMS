using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ContentPickerAlias, "Content Picker", "INT", "contentpicker", IsParameterEditor = true)]
    public class ContentPickerPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ContentPickerPreValueEditor();
        }

        internal class ContentPickerPreValueEditor : PreValueEditor
        {
            [PreValueField("startNodeId", "Start node", "treepicker")]
            public int StartNodeId { get; set; }
        }
    }
}
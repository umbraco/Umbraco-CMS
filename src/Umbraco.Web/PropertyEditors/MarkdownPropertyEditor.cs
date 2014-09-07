using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MarkdownEditorAlias, "Markdown editor", "markdowneditor", ValueType = "TEXT")]
    public class MarkdownPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MarkdownPreValueEditor();   
        }

        internal class MarkdownPreValueEditor : PreValueEditor
        {
            [PreValueField("preview", "Preview", "boolean", Description = "Display a live preview")]
            public bool DisplayLivePreview { get; set; }

            [PreValueField("defaultValue", "Default value", "textarea", Description = "If value is blank, the editor will show this")]
            public string DefaultValue { get; set; }
        }
    }
}
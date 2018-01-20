using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Aliases.MarkdownEditor, "Markdown editor", "markdowneditor", ValueType = PropertyEditorValueTypes.Text, Icon="icon-code", Group="rich content")]
    public class MarkdownPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public MarkdownPropertyEditor(ILogger logger) : base(logger)
        {
        }

        protected override PreValueEditor CreateConfigurationEditor()
        {
            return new MarkdownPreValueEditor();
        }

        internal class MarkdownPreValueEditor : PreValueEditor
        {
            [DataTypeConfigurationField("preview", "Preview", "boolean", Description = "Display a live preview")]
            public bool DisplayLivePreview { get; set; }

            [DataTypeConfigurationField("defaultValue", "Default value", "textarea", Description = "If value is blank, the editor will show this")]
            public string DefaultValue { get; set; }
        }
    }
}

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a markdown editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.MarkdownEditor, "Markdown editor", "markdowneditor", ValueType = ValueTypes.Text, Icon="icon-code", Group="rich content")]
    public class MarkdownPropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownPropertyEditor"/> class.
        /// </summary>
        public MarkdownPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MarkdownConfigurationEditor();
    }
}

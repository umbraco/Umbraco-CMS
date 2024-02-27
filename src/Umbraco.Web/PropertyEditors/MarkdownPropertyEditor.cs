using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a markdown editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MarkdownEditor,
        "Markdown editor",
        "markdowneditor",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.RichContent,
        Icon = "icon-code")]
    public class MarkdownPropertyEditor : DataEditor
    {
        private readonly IMarkdownSanitizer _markdownSanitizer;

        [Obsolete("Use non obsolete constructor")]
        public MarkdownPropertyEditor(ILogger logger)
            : this(logger, Current.Factory.GetInstance<IMarkdownSanitizer>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownPropertyEditor"/> class.
        /// </summary>
        public MarkdownPropertyEditor(ILogger logger, IMarkdownSanitizer markdownSanitizer)
            : base(logger)
        {
            _markdownSanitizer = markdownSanitizer;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MarkdownConfigurationEditor();

        /// <summary>
        ///     Create a custom value editor
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new MarkDownPropertyValueEditor(Attribute, _markdownSanitizer);
    }
}

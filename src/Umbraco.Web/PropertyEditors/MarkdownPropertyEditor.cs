using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

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
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownPropertyEditor"/> class.
        /// </summary>
        public MarkdownPropertyEditor(ILogger logger, IIOHelper ioHelper)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService, Current.Services.TextService,Current.ShortStringHelper)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MarkdownConfigurationEditor(_ioHelper);
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
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
        public MarkdownPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MarkdownConfigurationEditor(_ioHelper);
    }
}

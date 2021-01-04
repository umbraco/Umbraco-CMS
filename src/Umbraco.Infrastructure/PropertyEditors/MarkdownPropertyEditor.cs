using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

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
        public MarkdownPropertyEditor(
            ILoggerFactory loggerFactory,
            IIOHelper ioHelper,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MarkdownConfigurationEditor(_ioHelper);
    }
}

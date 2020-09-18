using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a list-view editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.ListView,
        "List view",
        "listview",
        HideLabel = true,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = Constants.Icons.ListView)]
    public class ListViewPropertyEditor : DataEditor
    {
        private readonly IIOHelper _iioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewPropertyEditor"/> class.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public ListViewPropertyEditor(
            ILoggerFactory loggerFactory,
            IIOHelper iioHelper,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            _iioHelper = iioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ListViewConfigurationEditor(_iioHelper);
    }
}

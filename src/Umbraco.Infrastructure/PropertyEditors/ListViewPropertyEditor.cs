using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Constants = Umbraco.Cms.Core.Constants;

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
        public ListViewPropertyEditor(
            ILoggerFactory loggerFactory,
            IIOHelper iioHelper,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(
                loggerFactory,
                dataTypeService,
                localizationService,
                localizedTextService,
                shortStringHelper,
                jsonSerializer)
        {
            _iioHelper = iioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ListViewConfigurationEditor(_iioHelper);
    }
}

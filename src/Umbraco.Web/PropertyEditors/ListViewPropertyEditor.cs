using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

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
        /// <param name="logger"></param>
        public ListViewPropertyEditor(ILogger logger, IIOHelper iioHelper, ILocalizationService localizationService)
            : base(logger, Current.Services.DataTypeService, localizationService, Current.Services.TextService,Current.ShortStringHelper)
        {
            _iioHelper = iioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ListViewConfigurationEditor(_iioHelper);
    }
}

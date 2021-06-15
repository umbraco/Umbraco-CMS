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
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper iioHelper)
            : base(dataValueEditorFactory)
        {
            _iioHelper = iioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ListViewConfigurationEditor(_iioHelper);
    }
}

using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a list-view editor.
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.Aliases.ListView, "List view", "listview", HideLabel = true, Group = "lists", Icon = "icon-item-arrangement")]
    public class ListViewPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewPropertyEditor"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public ListViewPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new ListViewConfigurationEditor();
        }

        public override IDictionary<string, object> DefaultPreValues
        {
            get
            {
                return new Dictionary<string, object>
                {
                    {"pageSize", "10"},
                    {"displayAtTabNumber", "1"},
                    {"orderBy", "SortOrder"},
                    {"orderDirection", "asc"},
                    {
                        "includeProperties", new[]
                        {
                            new {alias = "sortOrder", header = "Sort order", isSystem = 1},
                            new {alias = "updateDate", header = "Last edited", isSystem = 1},
                            new {alias = "owner", header = "Created by", isSystem = 1}
                        }
                    },
                    {
                        "layouts", new[]
                        {
                            new {name = "List", path = "views/propertyeditors/listview/layouts/list/list.html", icon = "icon-list", isSystem = 1, selected = true},
                            new {name = "Grid", path = "views/propertyeditors/listview/layouts/grid/grid.html", icon = "icon-thumbnails-small", isSystem = 1, selected = true}
                        }
                    },
                    {"bulkActionPermissions", new
                        {
                            allowBulkPublish = true,
                            allowBulkUnpublish = true,
                            allowBulkCopy = true,
                            allowBulkMove = false,
                            allowBulkDelete = true
                        }}
                };
            }
        }
    }
}

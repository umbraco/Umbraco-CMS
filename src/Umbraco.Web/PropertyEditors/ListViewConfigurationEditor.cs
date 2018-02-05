using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the listview value editor.
    /// </summary>
    public class ListViewConfigurationEditor : ConfigurationEditor<ListViewConfiguration>
    {
        public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
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
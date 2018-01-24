using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the listview value editor.
    /// </summary>
    public class ListViewConfiguration
    {
        [ConfigurationField("tabName", "Tab Name", "textstring", Description = "The name of the listview tab (default if empty: 'Child Items')")]
        public int TabName { get; set; }

        [ConfigurationField("displayAtTabNumber", "Display At Tab Number", "number", Description = "Which tab position that the list of child items will be displayed")]
        public int DisplayAtTabNumber { get; set; }

        [ConfigurationField("pageSize", "Page Size", "number", Description = "Number of items per page")]
        public int PageSize { get; set; }

        [ConfigurationField("layouts", "Layouts", "views/propertyeditors/listview/layouts.prevalues.html")]
        public int Layouts { get; set; }

        [ConfigurationField("includeProperties", "Columns Displayed", "views/propertyeditors/listview/includeproperties.prevalues.html",
            Description = "The properties that will be displayed for each column")]
        public object IncludeProperties { get; set; } // fixme object ?!

        [ConfigurationField("orderBy", "Order By", "views/propertyeditors/listview/sortby.prevalues.html",
            Description = "The default sort order for the list")]
        public int OrderBy { get; set; }

        [ConfigurationField("orderDirection", "Order Direction", "views/propertyeditors/listview/orderdirection.prevalues.html")]
        public int OrderDirection { get; set; }

        [ConfigurationField("bulkActionPermissions", "Bulk Action Permissions", "views/propertyeditors/listview/bulkactionpermissions.prevalues.html",
            Description = "The bulk actions that are allowed from the list view")]
        public BulkActionPermissionSettings BulkActionPermissions { get; set; }

        public class BulkActionPermissionSettings
        {
            public bool AllowBulkPublish { get; set; }

            public bool AllowBulkUnpublish { get; set; }

            public bool AllowBulkCopy { get; set; }

            public bool AllowBulkMove { get; set; }

            public bool AllowBulkDelete { get; set; }
        }
    }
}
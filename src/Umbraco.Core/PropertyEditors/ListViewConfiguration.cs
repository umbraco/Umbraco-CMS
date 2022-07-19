using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the listview value editor.
/// </summary>
public class ListViewConfiguration
{
    public ListViewConfiguration()
    {
        // initialize defaults
        PageSize = 10;
        OrderBy = "SortOrder";
        OrderDirection = "asc";

        BulkActionPermissions = new BulkActionPermissionSettings
        {
            AllowBulkPublish = true,
            AllowBulkUnpublish = true,
            AllowBulkCopy = true,
            AllowBulkMove = true,
            AllowBulkDelete = true,
        };

        Layouts = new[]
        {
            new Layout
            {
                Name = "List",
                Icon = "icon-list",
                IsSystem = 1,
                Selected = true,
                Path = "views/propertyeditors/listview/layouts/list/list.html",
            },
            new Layout
            {
                Name = "Grid",
                Icon = "icon-thumbnails-small",
                IsSystem = 1,
                Selected = true,
                Path = "views/propertyeditors/listview/layouts/grid/grid.html",
            },
        };

        IncludeProperties = new[]
        {
            new Property { Alias = "sortOrder", Header = "Sort order", IsSystem = 1 },
            new Property { Alias = "updateDate", Header = "Last edited", IsSystem = 1 },
            new Property { Alias = "owner", Header = "Created by", IsSystem = 1 },
        };
    }

    [ConfigurationField("pageSize", "Page Size", "number", Description = "Number of items per page")]
    public int PageSize { get; set; }

    [ConfigurationField("orderBy", "Order By", "views/propertyeditors/listview/sortby.prevalues.html", Description = "The default sort order for the list")]
    public string OrderBy { get; set; }

    [ConfigurationField("orderDirection", "Order Direction", "views/propertyeditors/listview/orderDirection.prevalues.html")]
    public string OrderDirection { get; set; }

    [ConfigurationField(
        "includeProperties",
        "Columns Displayed",
        "views/propertyeditors/listview/includeproperties.prevalues.html",
        Description = "The properties that will be displayed for each column")]
    public Property[] IncludeProperties { get; set; }

    [ConfigurationField("layouts", "Layouts", "views/propertyeditors/listview/layouts.prevalues.html")]
    public Layout[] Layouts { get; set; }

    [ConfigurationField(
        "bulkActionPermissions",
        "Bulk Action Permissions",
        "views/propertyeditors/listview/bulkActionPermissions.prevalues.html",
        Description = "The bulk actions that are allowed from the list view")]
    public BulkActionPermissionSettings BulkActionPermissions { get; set; } = new(); // TODO: managing defaults?

    [ConfigurationField("icon", "Content app icon", "views/propertyeditors/listview/icon.prevalues.html", Description = "The icon of the listview content app")]
    public string? Icon { get; set; }

    [ConfigurationField("tabName", "Content app name", "textstring", Description = "The name of the listview content app (default if empty: 'Child Items')")]
    public string? TabName { get; set; }

    [ConfigurationField(
        "showContentFirst",
        "Show Content App First",
        "boolean",
        Description = "Enable this to show the content app by default instead of the list view app")]
    public bool ShowContentFirst { get; set; }

    [ConfigurationField(
        "useInfiniteEditor",
        "Edit in Infinite Editor",
        "boolean",
        Description = "Enable this to use infinite editing to edit the content of the list view")]
    public bool UseInfiniteEditor { get; set; }

    [DataContract]
    public class Property
    {
        [DataMember(Name = "alias")]
        public string? Alias { get; set; }

        [DataMember(Name = "header")]
        public string? Header { get; set; }

        [DataMember(Name = "nameTemplate")]
        public string? Template { get; set; }

        [DataMember(Name = "isSystem")]
        public int IsSystem { get; set; } // TODO: bool
    }

    [DataContract]
    public class Layout
    {
        [DataMember(Name = "name")]
        public string? Name { get; set; }

        [DataMember(Name = "path")]
        public string? Path { get; set; }

        [DataMember(Name = "icon")]
        public string? Icon { get; set; }

        [DataMember(Name = "isSystem")]
        public int IsSystem { get; set; } // TODO: bool

        [DataMember(Name = "selected")]
        public bool Selected { get; set; }
    }

    [DataContract]
    public class BulkActionPermissionSettings
    {
        [DataMember(Name = "allowBulkPublish")]
        public bool AllowBulkPublish { get; set; } = true;

        [DataMember(Name = "allowBulkUnpublish")]
        public bool AllowBulkUnpublish { get; set; } = true;

        [DataMember(Name = "allowBulkCopy")]
        public bool AllowBulkCopy { get; set; } = true;

        [DataMember(Name = "allowBulkMove")]
        public bool AllowBulkMove { get; set; } = true;

        [DataMember(Name = "allowBulkDelete")]
        public bool AllowBulkDelete { get; set; } = true;
    }
}

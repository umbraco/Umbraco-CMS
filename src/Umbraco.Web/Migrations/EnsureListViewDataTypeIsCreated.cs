using System;
using NPoco;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Scoping;

namespace Umbraco.Web.Migrations
{
    /// <summary>
    /// Creates the built in list view data types
    /// </summary>
    public class EnsureDefaultListViewDataTypesCreated : IPostMigration
    {
        public void Execute(string name, IScope scope, SemVersion originVersion, SemVersion targetVersion, ILogger logger)
        {
            if (name != Constants.System.UmbracoUpgradePlanName) return;

            var target720 = new SemVersion(7, 2, 0);

            if (originVersion > target720)
                return;

            var syntax = scope.SqlContext.SqlSyntax;

            try
            {
                //Turn on identity insert if db provider is not mysql
                if (syntax.SupportsIdentityInsert())
                    scope.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", syntax.GetQuotedTableName("umbracoNode"))));

                if (scope.Database.Exists<NodeDto>(Constants.DataTypes.DefaultContentListView))
                {
                    //If this already exists then just exit
                    return;
                }

                scope.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.DataTypes.DefaultContentListView, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-95", SortOrder = 2, UniqueId = new Guid("C0808DD3-8133-4E4B-8CE8-E2BEA84A96A4"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Content", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
                scope.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.DataTypes.DefaultMediaListView, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-96", SortOrder = 2, UniqueId = new Guid("3A0156C4-3B8C-4803-BDC1-6871FAA83FFF"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Media", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
                scope.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = Constants.DataTypes.DefaultMembersListView, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,-97", SortOrder = 2, UniqueId = new Guid("AA2C52A0-CE87-4E65-A47C-7DF09358585D"), Text = Constants.Conventions.DataTypes.ListViewPrefix + "Members", NodeObjectType = Constants.ObjectTypes.DataType, CreateDate = DateTime.Now });
            }
            finally
            {
                //Turn off identity insert if db provider is not mysql
                if (syntax.SupportsIdentityInsert())
                    scope.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", syntax.GetQuotedTableName("umbracoNode"))));
            }

            try
            {
                //Turn on identity insert if db provider is not mysql
                if (syntax.SupportsIdentityInsert())
                    scope.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", syntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.DataType))));

                scope.Database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { PrimaryKey = -26, DataTypeId = Constants.DataTypes.DefaultContentListView, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });
                scope.Database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { PrimaryKey = -27, DataTypeId = Constants.DataTypes.DefaultMediaListView, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });
                scope.Database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, new DataTypeDto { PrimaryKey = -28, DataTypeId = Constants.DataTypes.DefaultMembersListView, PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias, DbType = "Nvarchar" });
            }
            finally
            {
                //Turn off identity insert if db provider is not mysql
                if (syntax.SupportsIdentityInsert())
                    scope.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", syntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.DataType))));
            }

            try
            {
                //Turn on identity insert if db provider is not mysql
                if (syntax.SupportsIdentityInsert())
                    scope.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", syntax.GetQuotedTableName("cmsDataTypePreValues"))));

                //defaults for the member list
                scope.Database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -1, Alias = "pageSize", SortOrder = 1, DataTypeNodeId = Constants.DataTypes.DefaultMembersListView, Value = "10" });
                scope.Database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -2, Alias = "orderBy", SortOrder = 2, DataTypeNodeId = Constants.DataTypes.DefaultMembersListView, Value = "Name" });
                scope.Database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -3, Alias = "orderDirection", SortOrder = 3, DataTypeNodeId = Constants.DataTypes.DefaultMembersListView, Value = "asc" });
                scope.Database.Insert("cmsDataTypePreValues", "id", false, new DataTypePreValueDto { Id = -4, Alias = "includeProperties", SortOrder = 4, DataTypeNodeId = Constants.DataTypes.DefaultMembersListView, Value = "[{\"alias\":\"email\",\"isSystem\":1},{\"alias\":\"username\",\"isSystem\":1},{\"alias\":\"updateDate\",\"header\":\"Last edited\",\"isSystem\":1}]" });
            }
            finally
            {
                //Turn off identity insert if db provider is not mysql
                if (syntax.SupportsIdentityInsert())
                    scope.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", syntax.GetQuotedTableName("cmsDataTypePreValues"))));
            }
        }
    }
}

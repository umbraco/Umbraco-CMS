using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using umbraco.interfaces;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// Once the migration runner is run, this will ensure that the content types that are flagged as list views,
    /// have the special readonly list view property type on the readonly tab. This will ensure that the built in system list view data type exists if the 
    /// current version is less than 7.2 (because previous to 7.2 we didn't ship with the system created list view data type)
    /// </summary>
    public class EnsureAllListViewContentTypesHaveListViewPropertyType : ApplicationEventHandler
    {
        /// <summary>
        /// Ensure this is run when not configured
        /// </summary>
        protected override bool ExecuteWhenApplicationNotConfigured
        {
            get { return true; }
        }

        /// <summary>
        /// Ensure this is run when not configured
        /// </summary>
        protected override bool ExecuteWhenDatabaseNotConfigured
        {
            get { return true; }
        }

        /// <summary>
        /// Attach to event on starting
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            MigrationRunner.Migrated += MigrationRunner_Migrated;
        }

        void MigrationRunner_Migrated(MigrationRunner sender, Core.Events.MigrationEventArgs e)
        {
            var target720 = new Version(7, 2, 0);

            if (e.ConfiguredVersion <= target720)
            {
                EnsureListViewDataTypeCreated(e);

                var services = ApplicationContext.Current.Services;

                var contentTypes = services.ContentTypeService.GetAllContentTypes().Where(x => x.IsContainer);                                
                services.ContentTypeService.Save(AddListView(contentTypes));

                var mediaTypes = services.ContentTypeService.GetAllMediaTypes().Where(x => x.IsContainer);
                services.ContentTypeService.Save(AddListView(mediaTypes));

                var memberTypes = services.MemberTypeService.GetAll().Where(x => x.IsContainer);
                services.MemberTypeService.Save(AddListView(memberTypes));
            }
        }

        private void EnsureListViewDataTypeCreated(Core.Events.MigrationEventArgs e)
        {
            var exists = e.MigrationContext.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE id=1037");
            if (exists > 0) return;

            using (var transaction = e.MigrationContext.Database.GetTransaction())
            {
                //Turn on identity insert if db provider is not mysql
                if (SqlSyntaxContext.SqlSyntaxProvider.SupportsIdentityInsert())
                    e.MigrationContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));

                e.MigrationContext.Database
                    .Insert("umbracoNode", "id", false, new NodeDto
                    {
                        NodeId = 1037,
                        Trashed = false,
                        ParentId = -1,
                        UserId = 0,
                        Level = 1,
                        Path = "-1,1037",
                        SortOrder = 2,
                        UniqueId = new Guid("C0808DD3-8133-4E4B-8CE8-E2BEA84A96A4"),
                        Text = "List View",
                        NodeObjectType = new Guid(Constants.ObjectTypes.DataType),
                        CreateDate = DateTime.Now
                    });

                //Turn off identity insert if db provider is not mysql
                if (SqlSyntaxContext.SqlSyntaxProvider.SupportsIdentityInsert())
                    e.MigrationContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));

                //Turn on identity insert if db provider is not mysql
                if (SqlSyntaxContext.SqlSyntaxProvider.SupportsIdentityInsert())
                    e.MigrationContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsDataType"))));

                e.MigrationContext.Database
                    .Insert("cmsDataType", "pk", false, new DataTypeDto
                    {
                        PrimaryKey = 19,
                        DataTypeId = 1037,
                        PropertyEditorAlias = Constants.PropertyEditors.ListViewAlias,
                        DbType = "Nvarchar"
                    });

                //Turn off identity insert if db provider is not mysql
                if (SqlSyntaxContext.SqlSyntaxProvider.SupportsIdentityInsert())
                    e.MigrationContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsDataType"))));

                transaction.Complete();
            }
        }

        private static List<T> AddListView<T>(IEnumerable<T> contentTypes)
            where T: IContentTypeBase
        {
            var toSave = new List<T>();
            foreach (var contentType in contentTypes)
            {
                if (contentType.PropertyGroups.Contains(Constants.Conventions.PropertyGroups.ListViewGroupName) == false)
                {
                    contentType.PropertyGroups.Add(new PropertyGroup
                    {
                        Name = Constants.Conventions.PropertyGroups.ListViewGroupName,                       
                        SortOrder = contentType.PropertyGroups.Any() ?  contentType.PropertyGroups.Max(x => x.SortOrder) + 1 : 1,
                        PropertyTypes = new PropertyTypeCollection(new[]
                        {
                            new PropertyType(Constants.PropertyEditors.ListViewAlias, DataTypeDatabaseType.Nvarchar)
                            {
                                Alias = Constants.Conventions.PropertyTypes.ListViewPropertyAlias,
                                Name = Constants.Conventions.PropertyTypes.ListViewPropertyAlias,
                                DataTypeDefinitionId = 1037
                            }
                        })
                    });
                    toSave.Add(contentType);
                }
            }
            return toSave;
        }
    }

}
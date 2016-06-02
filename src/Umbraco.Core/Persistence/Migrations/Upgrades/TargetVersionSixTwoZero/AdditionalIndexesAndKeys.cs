using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    [Migration("7.1.0", 1, GlobalSettings.UmbracoMigrationName)]
    [Migration("6.2.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class AdditionalIndexesAndKeys : MigrationBase
    {
        public AdditionalIndexesAndKeys(IMigrationContext context) 
            : base(context)
        { }


        public override void Up()
        {

            var dbIndexes = SqlSyntax.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            //do not create any indexes if they already exist in the database

            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoNodeTrashed")) == false)
            {
                Create.Index("IX_umbracoNodeTrashed").OnTable("umbracoNode").OnColumn("trashed").Ascending().WithOptions().NonClustered();
            }
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsContentVersion_ContentId")) == false)
            {
                Create.Index("IX_cmsContentVersion_ContentId").OnTable("cmsContentVersion").OnColumn("ContentId").Ascending().WithOptions().NonClustered();
            }
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsDocument_published")) == false)
            {
                Create.Index("IX_cmsDocument_published").OnTable("cmsDocument").OnColumn("published").Ascending().WithOptions().NonClustered();
            }
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsDocument_newest")) == false)
            {
                Create.Index("IX_cmsDocument_newest").OnTable("cmsDocument").OnColumn("newest").Ascending().WithOptions().NonClustered();
            }

            // drop the umbracoUserLogins_Index index since it is named incorrectly
            // and then re-create it so it follows the standard naming convention
            var sqlServerSyntax = SqlSyntax as SqlServerSyntaxProvider;
            if (sqlServerSyntax != null // if running sql server
                && sqlServerSyntax.ServerVersion.IsAzure // on Azure
                && sqlServerSyntax.ServerVersion.ProductVersion.StartsWith("11.")) // version 11.x
            {
                // SQL Azure v2 does not support dropping clustered indexes on a table
                // see http://issues.umbraco.org/issue/U4-5673            
                // and so we have to use a special method to do some manual work
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("umbracoUserLogins_Index")))
                {
                    //It's the old version that doesn't support dropping a clustered index on a table, so we need to do some manual work.
                    ExecuteSqlAzureSqlForChangingIndex();
                }
            }
            else
            {
                // any other db can delete and recreate

                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("umbracoUserLogins_Index")))
                {
                    Delete.Index("umbracoUserLogins_Index").OnTable("umbracoUserLogins");
                }
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoUserLogins_Index")) == false)
                {
                    Create.Index("IX_umbracoUserLogins_Index").OnTable("umbracoUserLogins").OnColumn("contextID").Ascending().WithOptions().Clustered();
                }
            }
        }

        public override void Down()
        {
            Delete.Index("IX_umbracoNodeTrashed").OnTable("umbracoNode");
            Delete.Index("IX_cmsContentVersion_ContentId").OnTable("cmsContentVersion");
            Delete.Index("IX_cmsDocument_published").OnTable("cmsDocument");
            Delete.Index("IX_cmsDocument_newest").OnTable("cmsDocument");
        }

        private void ExecuteSqlAzureSqlForChangingIndex()
        {
            Context.Database.Execute(@"CREATE TABLE ""umbracoUserLogins_temp""
(
	contextID uniqueidentifier NOT NULL,
	userID int NOT NULL,
	[timeout] bigint NOT NULL
);
CREATE CLUSTERED INDEX ""IX_umbracoUserLogins_Index"" ON ""umbracoUserLogins_temp"" (""contextID"");
INSERT INTO ""umbracoUserLogins_temp"" SELECT * FROM ""umbracoUserLogins""
DROP TABLE ""umbracoUserLogins""
CREATE TABLE ""umbracoUserLogins""
(
	contextID uniqueidentifier NOT NULL,
	userID int NOT NULL,
	[timeout] bigint NOT NULL
);
CREATE CLUSTERED INDEX ""IX_umbracoUserLogins_Index"" ON ""umbracoUserLogins"" (""contextID"");
INSERT INTO ""umbracoUserLogins"" SELECT * FROM ""umbracoUserLogins_temp""
DROP TABLE ""umbracoUserLogins_temp""");

        }
    }
}
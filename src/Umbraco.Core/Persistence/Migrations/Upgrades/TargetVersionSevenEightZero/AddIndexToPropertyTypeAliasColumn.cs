using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenEightZero
{
    [Migration("7.8.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddIndexToPropertyTypeAliasColumn : MigrationBase
    {
        public AddIndexToPropertyTypeAliasColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            Execute.Code(database =>
            {                
                var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(database);

                //make sure it doesn't already exist
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsPropertyTypeAlias")) == false)
                {
                    var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

                    //we can apply the index
                    localContext.Create.Index("IX_cmsPropertyTypeAlias").OnTable("cmsPropertyType")
                        .OnColumn("Alias")
                        .Ascending()
                        .WithOptions()
                        .NonClustered();

                    return localContext.GetSql();
                }

                return null;

            });


        }

        public override void Down()
        {
            Delete.Index("IX_cmsPropertyTypeAlias").OnTable("cmsPropertyType");
        }

    }
}

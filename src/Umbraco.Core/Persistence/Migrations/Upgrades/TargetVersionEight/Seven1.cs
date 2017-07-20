using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveFive;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight
{
    // catch-up with 7 migrations
    // fixme - this is temp and should be removed!

    [Migration("8.0.0", 100, Constants.System.UmbracoMigrationName)]
    public class Seven1 : MigrationBase
    {
        public Seven1(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            var migrations = new MigrationBase[]
            {
                // 7.5.0
                new RemoveStylesheetDataAndTablesAgain(Context),
                new TargetVersionSevenFiveZero.UpdateUniqueIndexOnCmsPropertyData(Context),

                // 7.5.5
                new UpdateAllowedMediaTypesAtRoot(Context),

                // 7.6.0
                new AddIndexesToUmbracoRelationTables(Context),
                new AddIndexToCmsMemberLoginName(Context),
                new AddIndexToUmbracoNodePath(Context),
                new AddMacroUniqueIdColumn(Context),
                new AddRelationTypeUniqueIdColumn(Context),
                new NormalizeTemplateGuids(Context),
                new ReduceLoginNameColumnsSize(Context),
                new TargetVersionSevenSixZero.UpdateUniqueIndexOnCmsPropertyData(Context),
                new RemoveUmbracoDeployTables(Context),
            };

            foreach (var migration in migrations)
                migration.Up();
        }
    }
}

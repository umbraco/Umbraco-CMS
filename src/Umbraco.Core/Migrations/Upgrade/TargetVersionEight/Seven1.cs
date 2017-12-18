using Umbraco.Core.Migrations.Upgrade.TargetVersionSevenFiveFive;
using Umbraco.Core.Migrations.Upgrade.TargetVersionSevenFiveZero;
using Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSevenZero;
using Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSixZero;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionEight
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
                new TargetVersionSevenFiveZero.UpdateUniqueIndexOnPropertyData(Context),

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
                new TargetVersionSevenSixZero.UpdateUniqueIndexOnPropertyData(Context),
                new RemoveUmbracoDeployTables(Context),

                // 7.7.0
                new AddIndexToDictionaryKeyColumn(Context),
                new AddUserGroupTables(Context),
                new AddUserStartNodeTable(Context),
                new EnsureContentTemplatePermissions(Context),
                new ReduceDictionaryKeyColumnsSize(Context),
                new UpdateUserTables(Context)
            };

            foreach (var migration in migrations)
                migration.Up();
        }
    }
}

using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwelveZero
{
    [Migration("7.12.0", 2, Constants.System.UmbracoMigrationName)]
    public class IncreaseLanguageIsoCodeColumnLength : MigrationBase
    {
        public IncreaseLanguageIsoCodeColumnLength(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(database =>
            {
                var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);
                // Some people seem to have a constraint in their DB instead of an index, we'd need to drop that one
                // See: https://our.umbraco.com/forum/using-umbraco-and-getting-started/93282-upgrade-from-711-to-712-fails
                var constraints = SqlSyntax.GetConstraintsPerTable(database).Distinct().ToArray();
                if (constraints.Any(x => x.Item2.InvariantEquals("IX_umbracoLanguage_languageISOCode")))
                {
                    localContext.Delete.UniqueConstraint("IX_umbracoLanguage_languageISOCode").FromTable("umbracoLanguage");
                    return localContext.GetSql();
                }
                return null;
            });

            Execute.Code(database =>
            {
                var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

                //Now check for indexes of that name and drop that if it exists
                var dbIndexes = SqlSyntax.GetDefinedIndexes(database)
                    .Select(x => new DbIndexDefinition(x)).ToArray();
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoLanguage_languageISOCode")))
                {
                    localContext.Delete.Index("IX_umbracoLanguage_languageISOCode").OnTable("umbracoLanguage");
                    return localContext.GetSql();
                }
                return null;
            });

            Alter.Table("umbracoLanguage")
                .AlterColumn("languageISOCode")
                .AsString(14)
                .Nullable();

            Create.Index("IX_umbracoLanguage_languageISOCode")
                .OnTable("umbracoLanguage")
                .OnColumn("languageISOCode")
                .Ascending()
                .WithOptions()
                .Unique();
        }

        public override void Down()
        {
        }
    }
}

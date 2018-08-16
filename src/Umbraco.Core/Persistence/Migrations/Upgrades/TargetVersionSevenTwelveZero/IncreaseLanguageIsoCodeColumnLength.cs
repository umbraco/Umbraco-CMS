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
            // Some people seem to have a constraint in their DB instead of an index, we'd need to drop that one
            // See: https://our.umbraco.com/forum/using-umbraco-and-getting-started/93282-upgrade-from-711-to-712-fails
            var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().Select(x => new ConstraintDefinition(ConstraintType.Unique)
            {
                TableName = x.Item1,
                SchemaName = x.Item2,
                ConstraintName = x.Item3
            }).ToArray();

            if (constraints.Any(x => x.ConstraintName.InvariantEquals("IX_umbracoLanguage_languageISOCode")))
            {
                Delete.UniqueConstraint("IX_umbracoLanguage_languageISOCode").FromTable("umbracoLanguage");
            }
            else
            {
                var dbIndexes = SqlSyntax.GetDefinedIndexes(Context.Database)
                    .Select(x => new DbIndexDefinition()
                    {
                        TableName = x.Item1,
                        IndexName = x.Item2,
                        ColumnName = x.Item3,
                        IsUnique = x.Item4
                    }).ToArray();

                //Ensure the index exists before dropping it
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoLanguage_languageISOCode")))
                {
                    Delete.Index("IX_umbracoLanguage_languageISOCode").OnTable("umbracoLanguage");
                }
            }

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

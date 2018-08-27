using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.V_7_12_0
{
    public class IncreaseLanguageIsoCodeColumnLength : MigrationBase
    {
        public IncreaseLanguageIsoCodeColumnLength(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // Some people seem to have a constraint in their DB instead of an index, we'd need to drop that one
            // See: https://our.umbraco.com/forum/using-umbraco-and-getting-started/93282-upgrade-from-711-to-712-fails
            var constraints = SqlSyntax.GetConstraintsPerTable(Context.Database).Distinct().ToArray();
            if (constraints.Any(x => x.Item2.InvariantEquals("IX_umbracoLanguage_languageISOCode")))
            {
                Delete.UniqueConstraint("IX_umbracoLanguage_languageISOCode").FromTable("umbracoLanguage").Do();
            }

            //Now check for indexes of that name and drop that if it exists
            var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(Context.Database);
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoLanguage_languageISOCode")))
            {
                Delete.Index("IX_umbracoLanguage_languageISOCode").OnTable("umbracoLanguage").Do();
            }

            Alter.Table("umbracoLanguage")
                .AlterColumn("languageISOCode")
                .AsString(14)
                .Nullable()
                .Do();

            Create.Index("IX_umbracoLanguage_languageISOCode")
                .OnTable("umbracoLanguage")
                .OnColumn("languageISOCode")
                .Ascending()
                .WithOptions()
                .Unique()
                .Do();
        }

    }
}

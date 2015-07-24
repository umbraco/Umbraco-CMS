using System;
using System.Data;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 14, GlobalSettings.UmbracoMigrationName)]
    public class AddForeignKeysForLanguageAndDictionaryTables : MigrationBase
    {
        public AddForeignKeysForLanguageAndDictionaryTables(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();

            //if the FK doesn't exist
            if (constraints.Any(x => x.Item1.InvariantEquals("cmsLanguageText") && x.Item2.InvariantEquals("languageId") && x.Item3.InvariantEquals("FK_cmsLanguageText_umbracoLanguage_id")) == false)
            {
                //Somehow, a language text item might end up with a language Id of zero or one that no longer exists
                //before we add the foreign key
                foreach (var pk in Context.Database.Query<int>(
                    "SELECT cmsLanguageText.pk FROM cmsLanguageText WHERE cmsLanguageText.languageId NOT IN (SELECT umbracoLanguage.id FROM umbracoLanguage)"))
                {
                    Delete.FromTable("cmsLanguageText").Row(new { pk = pk });
                }

                //now we need to create a foreign key
                Create.ForeignKey("FK_cmsLanguageText_umbracoLanguage_id").FromTable("cmsLanguageText").ForeignColumn("languageId")
                    .ToTable("umbracoLanguage").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);

                Alter.Table("cmsDictionary").AlterColumn("parent").AsGuid().Nullable();

                //set the parent to null if it equals the default dictionary item root id
                foreach (var pk in Context.Database.Query<int>("SELECT pk FROM cmsDictionary WHERE parent NOT IN (SELECT id FROM cmsDictionary)"))
                {
                    Update.Table("cmsDictionary").Set(new { parent = (Guid?)null }).Where(new { pk = pk });
                }

                Create.ForeignKey("FK_cmsDictionary_cmsDictionary_id").FromTable("cmsDictionary").ForeignColumn("parent")
                    .ToTable("cmsDictionary").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
            }


            
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}
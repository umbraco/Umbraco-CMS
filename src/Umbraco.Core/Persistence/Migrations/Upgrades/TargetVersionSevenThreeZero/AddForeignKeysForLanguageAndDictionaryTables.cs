using System;
using System.Data;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
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

                var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();

                if (columns.Any(x => x.ColumnName.InvariantEquals("id") 
                    && x.TableName.InvariantEquals("umbracoLanguage") 
                    && x.DataType.InvariantEquals("smallint")))
                {
                    //Ensure that the umbracoLanguage PK is INT and not SmallInt (which it  might be in older db versions)
                    // In order to 'change' this to an INT, we have to run a full migration script which is super annoying
                    Create.Table("umbracoLanguage_TEMP")
                        .WithColumn("id").AsInt32().NotNullable().Identity()
                        .WithColumn("languageISOCode").AsString(10).Nullable()
                        .WithColumn("languageCultureName").AsString(50).Nullable();
                
                    var currentData = this.Context.Database.Fetch<LanguageDto>(new Sql().Select("*").From<LanguageDto>(SqlSyntax));
                    foreach (var languageDto in currentData)
                    {
                        Insert.IntoTable("umbracoLanguage_TEMP")
                            .EnableIdentityInsert()
                            .Row(new {id = languageDto.Id, languageISOCode = languageDto.IsoCode, languageCultureName = languageDto.CultureName});
                    }
                    
                    //ok, all data has been copied over, drop the old table, rename the temp table and re-add constraints.
                    Delete.Table("umbracoLanguage");
                    Rename.Table("umbracoLanguage_TEMP").To("umbracoLanguage");

                    //add the pk
                    Create.PrimaryKey("PK_language").OnTable("umbracoLanguage").Column("id");
                }

                var dbIndexes = SqlSyntax.GetDefinedIndexes(Context.Database)
                    .Select(x => new DbIndexDefinition
                    {
                        TableName = x.Item1,
                        IndexName = x.Item2,
                        ColumnName = x.Item3,
                        IsUnique = x.Item4
                    }).ToArray();

                //make sure it doesn't already exist
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsDictionary_id")) == false)
                {
                    Create.Index("IX_cmsDictionary_id").OnTable("cmsDictionary")
                        .OnColumn("id").Ascending()
                        .WithOptions().NonClustered()
                        .WithOptions().Unique();
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
            throw new NotImplementedException();
        }
    }
}
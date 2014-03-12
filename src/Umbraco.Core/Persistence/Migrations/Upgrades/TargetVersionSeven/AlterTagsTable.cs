using System;
using System.Data;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 9, GlobalSettings.UmbracoMigrationName)]
    public class AlterTagsTable : MigrationBase
    {
        public override void Up()
        {
            var dbIndexes = SqlSyntaxContext.SqlSyntaxProvider.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            var constraints = SqlSyntaxContext.SqlSyntaxProvider.GetConstraintsPerColumn(Context.Database).DistinctBy(x => x.Item3).ToList();

            //make sure it doesn't already exist
            if (constraints.Any(x => x.Item3 == "FK_cmsTags_cmsTags") == false)
            {
                //add a foreign key to the parent id column too!
                Create.ForeignKey("FK_cmsTags_cmsTags")
                      .FromTable("cmsTags")
                      .ForeignColumn("ParentId")
                      .ToTable("cmsTags")
                      .PrimaryColumn("id")
                      .OnDelete(Rule.None)
                      .OnUpdate(Rule.None);    
            }

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName == "IX_cmsTags") == false)
            {
                //add an index to tag/group since it's queried often
                Create.Index("IX_cmsTags").OnTable("cmsTags").OnColumn("tag").Ascending().OnColumn("group").Ascending().WithOptions().NonClustered();
            }
            
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
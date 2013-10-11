using System;
using System.Data;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 9, GlobalSettings.UmbracoMigrationName)]
    public class AlterTagsTable : MigrationBase
    {
        public override void Up()
        {
            //add a foreign key to the parent id column too!
            Create.ForeignKey("FK_cmsTags_cmsTags")
                  .FromTable("cmsTags")
                  .ForeignColumn("ParentId")
                  .ToTable("cmsTags")
                  .PrimaryColumn("id")
                  .OnDelete(Rule.None)
                  .OnUpdate(Rule.None);

            //add an index to tag/group since it's queried often
            Create.Index("IX_cmsTags").OnTable("cmsTags").OnColumn("tag").Ascending().OnColumn("group").Ascending().WithOptions().NonClustered();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
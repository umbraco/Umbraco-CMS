using System;
using System.Data;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 10, GlobalSettings.UmbracoMigrationName)]
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
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
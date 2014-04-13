﻿using System.Data;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionFourNineZero
{
    [MigrationAttribute("4.9.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class RemoveUmbracoAppConstraints : MigrationBase
    {
        public override void Up()
        {
            //This will work on mysql and should work on mssql however the old keys were not named consistently with how the keys are 
            // structured now. So we need to do a check and manually remove them based on their old aliases.

            if (this.Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                Delete.ForeignKey().FromTable("umbracoUser2app").ForeignColumn("app").ToTable("umbracoApp").PrimaryColumn("appAlias");
                Delete.ForeignKey().FromTable("umbracoAppTree").ForeignColumn("appAlias").ToTable("umbracoApp").PrimaryColumn("appAlias");
            }
            else
            {
                //These are the old aliases
                Delete.ForeignKey("FK_umbracoUser2app_umbracoApp").OnTable("umbracoUser2app");
                Delete.ForeignKey("FK_umbracoUser2app_umbracoUser").OnTable("umbracoUser2app");
            }
            
        }

        public override void Down()
        {
            Create.ForeignKey("FK_umbracoUser2app_umbracoApp").FromTable("umbracoUser2app").ForeignColumn("app")
                .ToTable("umbracoApp").PrimaryColumn("appAlias").OnDeleteOrUpdate(Rule.None);

            Create.ForeignKey("FK_umbracoAppTree_umbracoApp").FromTable("umbracoAppTree").ForeignColumn("appAlias")
                .ToTable("umbracoApp").PrimaryColumn("appAlias").OnDeleteOrUpdate(Rule.None);
        }
    }
}
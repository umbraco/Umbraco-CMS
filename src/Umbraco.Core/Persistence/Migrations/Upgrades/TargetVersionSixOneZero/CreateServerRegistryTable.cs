using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixOneZero
{
   
    [Migration("6.1.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class CreateServerRegistryTable : MigrationBase
    {
        public override void Up()
        {
            var schemaHelper = new DatabaseSchemaHelper(Context.Database, Logger, SqlSyntax);

            //NOTE: This isn't the correct way to do this but to manually create this table with the Create syntax is a pain in the arse
            schemaHelper.CreateTable<ServerRegistrationDto>();            
        }

        public override void Down()
        {
            Delete.Table("umbracoServer");
        }
    }
}

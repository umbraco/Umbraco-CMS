using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixOneZero
{
    //NOTE: SD: Commenting out for now until we want to release a distributed cache provider that 
    // uses internal DNS names for each website to 'call' home intead of the current configuration based approach.

    //[Migration("6.1.0", 0, GlobalSettings.UmbracoMigrationName)]
    //public class CreateServerRegistryTable : MigrationBase
    //{
    //    public override void Up()
    //    {
    //        base.Context.Database.CreateTable<ServerRegistrationDto>();
    //    }

    //    public override void Down()
    //    {
    //    }
    //}
}

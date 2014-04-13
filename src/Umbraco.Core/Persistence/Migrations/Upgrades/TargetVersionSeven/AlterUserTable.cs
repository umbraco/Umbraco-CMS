﻿using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 3, GlobalSettings.UmbracoMigrationName)]
    public class AlterUserTable : MigrationBase
    {
        public override void Up()
        {
            Delete.Column("userDefaultPermissions").FromTable("umbracoUser");

            //"[DF_umbracoUser_defaultToLiveEditing]""
            Delete.DefaultConstraint().OnTable("umbracoUser").OnColumn("defaultToLiveEditing");
            Delete.Column("defaultToLiveEditing").FromTable("umbracoUser");
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }
    }
}
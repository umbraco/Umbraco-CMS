﻿using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class DropControlIdColumn : MigrationBase
    {
        public override void Up()
        {
            Delete.Column("controlId").FromTable("cmsDataType");
            //drop the default contstraint on the new column too
            Delete.DefaultConstraint().OnTable("cmsDataType").OnColumn("propertyEditorAlias");
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }
    }
}
﻿using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{

    [Migration("7.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AddPropertyEditorAliasColumn : MigrationBase
    {
        public override void Up()
        {               
            Alter.Table("cmsDataType").AddColumn("propertyEditorAlias").AsString(255).NotNullable().WithDefaultValue("");
        }

        public override void Down()
        {
            Delete.Column("propertyEditorAlias").FromTable("cmsDataType");
        }
    }
}
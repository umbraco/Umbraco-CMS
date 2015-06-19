using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{

    [Migration("7.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AddPropertyEditorAliasColumn : MigrationBase
    {
        public AddPropertyEditorAliasColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

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
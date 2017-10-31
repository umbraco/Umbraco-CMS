using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{

    [Migration("7.0.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddPropertyEditorAliasColumn : MigrationBase
    {
        public AddPropertyEditorAliasColumn(IMigrationContext context)
            : base(context)
        { }


        public override void Up()
        {
            Alter.Table(Constants.DatabaseSchema.Tables.DataType).AddColumn("propertyEditorAlias").AsString(255).NotNullable().WithDefaultValue("");
        }

        public override void Down()
        {
            Delete.Column("propertyEditorAlias").FromTable(Constants.DatabaseSchema.Tables.DataType);
        }
    }
}

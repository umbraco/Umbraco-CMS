using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 7, GlobalSettings.UmbracoMigrationName)]
    public class RemoveCmsMacroPropertyTypeTable : MigrationBase
    {
        public RemoveCmsMacroPropertyTypeTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Delete.Table("cmsMacroPropertyType");
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }
    }
}
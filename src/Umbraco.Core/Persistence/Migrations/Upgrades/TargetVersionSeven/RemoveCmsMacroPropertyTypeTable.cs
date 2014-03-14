using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 7, GlobalSettings.UmbracoMigrationName)]
    public class RemoveCmsMacroPropertyTypeTable : SchemaMigration
    {
        public override void Up()
        {
            Delete.Table("cmsMacroPropertyType");
        }

        public override void Down()
        {
            throw new CatastrophicDataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }
    }
}
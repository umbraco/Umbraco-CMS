using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_13_0
{
    public class AddPropertyTypeTabsTable : MigrationBase
    {
        public AddPropertyTypeTabsTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);
            if (tables.InvariantContains(Constants.DatabaseSchema.Tables.PropertyTypeTab)) return;

            // A brand new table/DTO for storing Tabs
            // and references to Proprty Types/DocTypes/Content Types
            Create.Table<PropertyTypeTabDto>(true).Do();
        }
    }
}

namespace Umbraco.Core.Migrations.Upgrade.V_8_8_0
{
    public class AddDomainSortOrder : MigrationBase
    {
        public AddDomainSortOrder(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            if (!ColumnExists(Constants.DatabaseSchema.Tables.Domain, "sortOrder"))
            {
                // Create sortOrder with default value as it's not nullable
                Create
                    .Column("sortOrder")
                    .OnTable(Constants.DatabaseSchema.Tables.Domain)
                    .AsInt32()
                    .NotNullable()
                    .WithDefaultValue(0)
                    .Do();

                // Delete the default value constraint
                Delete
                    .DefaultConstraint()
                    .OnTable(Constants.DatabaseSchema.Tables.Domain)
                    .OnColumn("sortOrder")
                    .Do();
            }
        }
    }
}

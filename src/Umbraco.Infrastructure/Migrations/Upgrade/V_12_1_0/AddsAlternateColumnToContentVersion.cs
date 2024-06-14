using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_1_0;

public class AddsAlternateColumnToContentVersion : MigrationBase
{
    public AddsAlternateColumnToContentVersion(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (ColumnExists(ContentVersionDto.TableName, "alternate") is false)
        {
            Create.Column("alternate")
                .OnTable(Constants.DatabaseSchema.Tables.ContentVersion)
                .AsBoolean()
                .WithDefaultValue(0)
                .Do();
        }
    }
}

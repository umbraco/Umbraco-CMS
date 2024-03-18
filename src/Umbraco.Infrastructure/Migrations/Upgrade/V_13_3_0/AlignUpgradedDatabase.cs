using NPoco;
using ColumnInfo = Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_3_0;

/// <summary>
/// We see some differences between an updated database and a fresh one,
/// the purpose of this migration is to align the two.
/// </summary>
public class AlignUpgradedDatabase : MigrationBase
{
    public AlignUpgradedDatabase(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        // We ignore SQLite since it's considered a development DB

        if (DatabaseType == DatabaseType.SQLite)
        {
            return;
        }

        IEnumerable<ColumnInfo> columns = SqlSyntax.GetColumnsInSchema(Context.Database);

        const string cacheTableName = "umbracoCacheInstruction";
        const string cacheColumnName = "jsonInstruction";
        ColumnInfo? jsonInstructionColumn = columns
            .FirstOrDefault(x => x is { TableName: cacheTableName, ColumnName: cacheColumnName });

        if (jsonInstructionColumn is null)
        {
            throw new InvalidOperationException("Could not find cache instruction column");
        }

        if (jsonInstructionColumn.ColumnDefault != null)
        {
            Delete.DefaultConstraint()
                .OnTable(cacheTableName)
                .OnColumn(cacheColumnName).Do();
        }
    }
}

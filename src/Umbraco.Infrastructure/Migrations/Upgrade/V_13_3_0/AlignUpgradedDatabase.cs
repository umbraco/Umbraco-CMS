﻿using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;
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

        ColumnInfo[] columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

        DropCacheInstructionDefaultConstraint(columns);
        RenameVersionDateColumn(columns);

    }

    private void DropCacheInstructionDefaultConstraint(IEnumerable<ColumnInfo> columns)
    {
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

    private void RenameVersionDateColumn(IEnumerable<ColumnInfo> columns)
    {
        const string tableName = "umbracoContentVersion";
        const string columnName = "VersionDate";
        ColumnInfo? versionDateColumn = columns
            .FirstOrDefault(x => x is { TableName: tableName, ColumnName: columnName });

        if (versionDateColumn is null)
        {
            // The column was not found I.E. the column is correctly named
            return;
        }

        Rename.Column(columnName)
            .OnTable(tableName)
            .To("versionDate")
            .Do();

        // Renames the default constraint for the column,
        // apparently the content version table used to be prefixed with cms and not umbraco
        // We don't have a fluid way to rename the default constraint so we have to use raw SQL
        // This should be okay though since we are only running this migration on SQL Server
        Sql<ISqlContext> renameConstraintQuery = Database.SqlContext.Sql(
            "EXEC sp_rename N'DF_cmsContentVersion_VersionDate', N'DF_umbracoContentVersion_versionDate', N'OBJECT'");
        Database.Execute(renameConstraintQuery);
    }
}

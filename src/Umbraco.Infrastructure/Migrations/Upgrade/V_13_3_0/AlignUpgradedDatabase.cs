using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
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
        AlignContentVersionTable(columns);
        UpdateExternalLoginIndexes();
        AlignNodeTable(columns);
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

    private void AlignContentVersionTable(ColumnInfo[] columns)
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

        RenameColumn(tableName, columnName, "versionDate", columns);

        // Renames the default constraint for the column,
        // apparently the content version table used to be prefixed with cms and not umbraco
        // We don't have a fluid way to rename the default constraint so we have to use raw SQL
        // This should be okay though since we are only running this migration on SQL Server
        Sql<ISqlContext> renameConstraintQuery = Database.SqlContext.Sql(
            "EXEC sp_rename N'DF_cmsContentVersion_VersionDate', N'DF_umbracoContentVersion_versionDate', N'OBJECT'");
        Database.Execute(renameConstraintQuery);
    }

    private void RenameColumn(string tableName, string oldColumnName, string newColumnName, IEnumerable<ColumnInfo> columns)
    {
        ColumnInfo? targetColumn = columns
            .FirstOrDefault(x => x.TableName == tableName && x.ColumnName == oldColumnName);

        if (targetColumn is null)
        {
            // The column was not found I.E. the column is correctly named
            return;
        }

        Rename.Column(oldColumnName)
            .OnTable(tableName)
            .To(newColumnName)
            .Do();
    }

    private void UpdateExternalLoginIndexes()
    {
        const string loginProviderIndexName = "IX_umbracoExternalLogin_LoginProvider";
        const string userMemberOrKeyIndexName = "IX_umbracoExternalLogin_userOrMemberKey";

        // Indexes are in format TableName, IndexName, ColumnName, IsUnique
        IEnumerable<Tuple<string, string, string, bool>> indexes = SqlSyntax.GetDefinedIndexes(Database);

        // Let's only mess with the indexes if we have to.
        Tuple<string, string, string, bool>? loginProviderIndex = indexes.FirstOrDefault(x =>
            x is { Item1: "umbracoExternalLogin", Item2: loginProviderIndexName });

        if (loginProviderIndex?.Item4 is false)
        {
            // The recommended way to change an index from non-unique to unique is to drop and recreate it.
            DeleteIndex<ExternalLoginDto>(loginProviderIndexName);
            CreateIndex<ExternalLoginDto>(loginProviderIndexName);
        }

        if (IndexExists(userMemberOrKeyIndexName))
        {
            return;
        }

        CreateIndex<ExternalLoginDto>(userMemberOrKeyIndexName);
    }

    private void AlignNodeTable(ColumnInfo[] columns)
    {
        const string tableName = "umbracoNode";
        RenameColumn(tableName, "parentID", "parentId", columns);
        RenameColumn(tableName, "uniqueID", "uniqueId", columns);

        const string extraIndexName = "IX_umbracoNode_ParentId";
        if (IndexExists(extraIndexName))
        {
            DeleteIndex<NodeDto>(extraIndexName);
        }
    }
}

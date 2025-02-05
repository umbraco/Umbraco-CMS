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
    public AlignUpgradedDatabase(IMigrationContext context)
        : base(context)
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
        Tuple<string, string, string, bool>[] indexes = SqlSyntax.GetDefinedIndexes(Database).ToArray();

        DropCacheInstructionDefaultConstraint(columns);
        AlignContentVersionTable(columns);
        UpdateExternalLoginIndexes(indexes);
        AlignNodeTable(columns);
        MakeRelationTypeIndexUnique(indexes);
        RemoveUserGroupDefault(columns);
        MakeWebhookUrlNotNullable(columns);
        MakeWebhookLogUrlNotNullable(columns);
    }

    private void MakeIndexUnique<TDto>(string tableName, string indexName, IEnumerable<Tuple<string, string, string, bool>> indexes)
    {
        // Let's only mess with the indexes if we have to.
        // Indexes are in format TableName, IndexName, ColumnName, IsUnique
        Tuple<string, string, string, bool>? loginProviderIndex = indexes.FirstOrDefault(x =>
            x.Item1 == tableName && x.Item2 == indexName);

        // Item4 == IsUnique
        if (loginProviderIndex?.Item4 is false)
        {
            // The recommended way to change an index from non-unique to unique is to drop and recreate it.
            DeleteIndex<TDto>(indexName);
            CreateIndex<TDto>(indexName);
        }
    }

    private void RemoveDefaultConstraint(string tableName, string columnName, IEnumerable<ColumnInfo> columns)
    {
        ColumnInfo? targetColumn = columns
            .FirstOrDefault(x => x.TableName == tableName && x.ColumnName == columnName);

        if (targetColumn is null)
        {
            throw new InvalidOperationException($"Could not find {columnName} column on {tableName} table.");
        }

        if (targetColumn.ColumnDefault is null)
        {
            return;
        }

        Delete.DefaultConstraint()
            .OnTable(tableName)
            .OnColumn(columnName)
            .Do();
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

    private void MakeNvarCharColumnNotNullable(string tableName, string columnName, IEnumerable<ColumnInfo> columns)
    {
        ColumnInfo? targetColumn = columns.FirstOrDefault(x => x.TableName == tableName && x.ColumnName == columnName);

        if (targetColumn is null)
        {
            throw new InvalidOperationException($"Could not find {columnName} column in {tableName} table.");
        }

        if (targetColumn.IsNullable is false)
        {
            return;
        }

        Alter.Table(tableName)
            .AlterColumn(columnName)
            .AsCustom("nvarchar(max)")
            .NotNullable()
            .Do();
    }

    private void DropCacheInstructionDefaultConstraint(IEnumerable<ColumnInfo> columns)
        => RemoveDefaultConstraint("umbracoCacheInstruction", "jsonInstruction", columns);

    private void AlignContentVersionTable(ColumnInfo[] columns)
    {
        // We need to do this to ensure we don't try to rename the constraint if it doesn't exist.
        const string tableName = "umbracoContentVersion";
        const string columnName = "VersionDate";
        const string newColumnName = "versionDate";
        const string expectedConstraintName = "DF_umbracoContentVersion_versionDate";

        ColumnInfo? versionDateColumn = columns
            .FirstOrDefault(x => x is { TableName: tableName, ColumnName: columnName });

        // we only want to rename the column if necessary
        if (versionDateColumn is not null)
        {
            // The column was not found I.E. the column is correctly named
            RenameColumn(tableName, columnName, newColumnName, columns);
        }

        // Renames the default constraint for the column,
        // apparently the content version table used to be prefixed with cms and not umbraco
        // We don't have a fluid way to rename the default constraint so we have to use raw SQL
        // This should be okay though since we are only running this migration on SQL Server
        Sql<ISqlContext> constraintNameQuery = Database.SqlContext.Sql(@$"
SELECT obj_Constraint.NAME AS 'constraintName'
    FROM   sys.objects obj_table
        JOIN sys.objects obj_Constraint
            ON obj_table.object_id = obj_Constraint.parent_object_id
        JOIN sys.sysconstraints constraints
             ON constraints.constid = obj_Constraint.object_id
        JOIN sys.columns columns
             ON columns.object_id = obj_table.object_id
            AND columns.column_id = constraints.colid
    WHERE obj_table.NAME = '{tableName}'
	AND columns.NAME = '{newColumnName}'
	AND obj_Constraint.type = 'D'
");
        var currentConstraintName = Database.ExecuteScalar<string>(constraintNameQuery);

        // Only rename the constraint if necessary.
        if (currentConstraintName == expectedConstraintName)
        {
            return;
        }

        if (currentConstraintName is null)
        {
            // Constraint does not exist, so we need to create it.
            Sql<ISqlContext> createConstraintStatement = Database.SqlContext.Sql(@$"
ALTER TABLE umbracoContentVersion ADD CONSTRAINT [DF_umbracoContentVersion_versionDate] DEFAULT (getdate()) FOR [versionDate]");
            Database.Execute(createConstraintStatement);
        }
        else
        {
            // Constraint exists, and differs from the expected name, so we need to rename it.
            Sql<ISqlContext> renameConstraintQuery = Database.SqlContext.Sql(
                $"EXEC sp_rename N'{currentConstraintName}', N'{expectedConstraintName}', N'OBJECT'");
            Database.Execute(renameConstraintQuery);
        }
    }

    private void UpdateExternalLoginIndexes(IEnumerable<Tuple<string, string, string, bool>> indexes)
    {
        const string userMemberOrKeyIndexName = "IX_umbracoExternalLogin_userOrMemberKey";

        MakeIndexUnique<ExternalLoginDto>("umbracoExternalLogin", "IX_umbracoExternalLogin_LoginProvider", indexes);

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

    private void MakeRelationTypeIndexUnique(Tuple<string, string, string, bool>[] indexes)
        => MakeIndexUnique<RelationTypeDto>("umbracoRelationType", "IX_umbracoRelationType_alias", indexes);

    private void RemoveUserGroupDefault(ColumnInfo[] columns)
        => RemoveDefaultConstraint("umbracoUserGroup", "hasAccessToAllLanguages", columns);

    private void MakeWebhookUrlNotNullable(ColumnInfo[] columns)
        => MakeNvarCharColumnNotNullable("umbracoWebhook", "url", columns);

    private void MakeWebhookLogUrlNotNullable(ColumnInfo[] columns)
        => MakeNvarCharColumnNotNullable("umbracoWebhookLog", "url", columns);
}

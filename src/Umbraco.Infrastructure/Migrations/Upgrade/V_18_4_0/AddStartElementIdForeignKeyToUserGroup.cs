using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_4_0;

/// <summary>
///     Adds the foreign key from <c>umbracoUserGroup.startElementId</c> to <c>umbracoNode</c>.
/// </summary>
/// <remarks>
///     The column was added on upgrade without its foreign key, so only clean installs have it. Start element ids
///     that no longer reference a node are cleared, as the key can't be added while they exist.
/// </remarks>
public class AddStartElementIdForeignKeyToUserGroup : AsyncMigrationBase
{
    private const string PreviousColumnName = UserGroupDto.StartElementIdColumnName + "Previous";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddStartElementIdForeignKeyToUserGroup"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddStartElementIdForeignKeyToUserGroup(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (ForeignKeyExists())
        {
            return Task.CompletedTask;
        }

        if (DatabaseType == DatabaseType.SQLite)
        {
            RecreateColumnWithForeignKeyOnSqlite();
        }
        else
        {
            AddForeignKey();
        }

        return Task.CompletedTask;
    }

    private bool ForeignKeyExists()
        => SqlSyntax.GetConstraintsPerColumn(Context.Database).Any(x =>
            x.Item1.InvariantEquals(UserGroupDto.TableName) && x.Item2.InvariantEquals(UserGroupDto.StartElementIdColumnName));

    private void AddForeignKey()
    {
        var userGroupTable = SqlSyntax.GetQuotedTableName(UserGroupDto.TableName);
        var startElementId = SqlSyntax.GetQuotedColumnName(UserGroupDto.StartElementIdColumnName);

        Execute.Sql($"UPDATE {userGroupTable} SET {startElementId} = NULL WHERE {startElementId} IS NOT NULL AND NOT {NodeExists(UserGroupDto.StartElementIdColumnName)}").Do();

        Create.ForeignKey(UserGroupDto.StartElementIdForeignKeyName)
            .FromTable(UserGroupDto.TableName)
            .ForeignColumn(UserGroupDto.StartElementIdColumnName)
            .ToTable(NodeDto.TableName)
            .PrimaryColumn(NodeDto.PrimaryKeyColumnName)
            .Do();
    }

    // SQLite can't add a foreign key to an existing column, so the column is added again with the key declared on it,
    // and the start element ids that still reference a node are copied across.
    private void RecreateColumnWithForeignKeyOnSqlite()
    {
        var userGroupTable = SqlSyntax.GetQuotedTableName(UserGroupDto.TableName);
        var startElementId = SqlSyntax.GetQuotedColumnName(UserGroupDto.StartElementIdColumnName);
        var previousStartElementId = SqlSyntax.GetQuotedColumnName(PreviousColumnName);

        TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(UserGroupDto), SqlSyntax);
        ColumnDefinition column = table.Columns.First(x => x.Name.InvariantEquals(UserGroupDto.StartElementIdColumnName));
        var columnSql = $"{SqlSyntax.Format(column)} CONSTRAINT {UserGroupDto.StartElementIdForeignKeyName} REFERENCES "
            + $"{SqlSyntax.GetQuotedTableName(NodeDto.TableName)} ({SqlSyntax.GetQuotedColumnName(NodeDto.PrimaryKeyColumnName)})";

        Execute.Sql(string.Format(SqlSyntax.RenameColumn, userGroupTable, startElementId, previousStartElementId)).Do();
        Execute.Sql(string.Format(SqlSyntax.AddColumn, userGroupTable, columnSql)).Do();
        Execute.Sql($"UPDATE {userGroupTable} SET {startElementId} = {previousStartElementId} WHERE {NodeExists(PreviousColumnName)}").Do();
        Execute.Sql(string.Format(SqlSyntax.DropColumn, userGroupTable, previousStartElementId)).Do();
    }

    private string NodeExists(string userGroupColumnName)
    {
        var nodeTable = SqlSyntax.GetQuotedTableName(NodeDto.TableName);
        var nodeId = SqlSyntax.GetQuotedColumnName(NodeDto.PrimaryKeyColumnName);
        var userGroupColumn = $"{SqlSyntax.GetQuotedTableName(UserGroupDto.TableName)}.{SqlSyntax.GetQuotedColumnName(userGroupColumnName)}";

        return $"EXISTS (SELECT 1 FROM {nodeTable} WHERE {nodeTable}.{nodeId} = {userGroupColumn})";
    }
}

using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Adds the <c>startDocumentBlueprintId</c> column to the <c>umbracoUserGroup</c> table.
/// </summary>
/// <remarks>
///     Runs as a premigration: signing in loads the user's groups, and every user group query selects this
///     column, so it has to exist before the main plan runs. Only the column and its foreign key are added here.
///     Deciding which groups keep blueprint access is left to
///     <see cref="GrantDocumentBlueprintAccessToSettingsGroups"/> in the main plan, so nothing about who can do
///     what changes before an upgrade is approved.
/// </remarks>
public class AddDocumentBlueprintStartNodeToUserGroup : AsyncMigrationBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddDocumentBlueprintStartNodeToUserGroup"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddDocumentBlueprintStartNodeToUserGroup(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (ColumnExists(UserGroupDto.TableName, UserGroupDto.StartDocumentBlueprintIdColumnName))
        {
            return Task.CompletedTask;
        }

        if (DatabaseType == DatabaseType.SQLite)
        {
            AddColumnWithForeignKeyOnSqlite();
        }
        else
        {
            AddColumnWithForeignKey();
        }

        return Task.CompletedTask;
    }

    private void AddColumnWithForeignKey()
    {
        AddColumn<UserGroupDto>(UserGroupDto.TableName, UserGroupDto.StartDocumentBlueprintIdColumnName);

        // AddColumn doesn't create the foreign key today. Check it doesn't already exist, in case that changes.
        if (SqlSyntax.GetConstraintsPerColumn(Context.Database).Any(x =>
                x.Item1.InvariantEquals(UserGroupDto.TableName) && x.Item2.InvariantEquals(UserGroupDto.StartDocumentBlueprintIdColumnName)))
        {
            return;
        }

        Create.ForeignKey(UserGroupDto.StartDocumentBlueprintIdForeignKeyName)
            .FromTable(UserGroupDto.TableName)
            .ForeignColumn(UserGroupDto.StartDocumentBlueprintIdColumnName)
            .ToTable(NodeDto.TableName)
            .PrimaryColumn(NodeDto.PrimaryKeyColumnName)
            .Do();
    }

    // SQLite can't add a foreign key to an existing column, so it has to be declared when the column is added.
    private void AddColumnWithForeignKeyOnSqlite()
    {
        TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(UserGroupDto), SqlSyntax);
        ColumnDefinition column = table.Columns.First(x => x.Name.InvariantEquals(UserGroupDto.StartDocumentBlueprintIdColumnName));

        var columnSql = $"{SqlSyntax.Format(column)} CONSTRAINT {UserGroupDto.StartDocumentBlueprintIdForeignKeyName} REFERENCES "
            + $"{SqlSyntax.GetQuotedTableName(NodeDto.TableName)} ({SqlSyntax.GetQuotedColumnName(NodeDto.PrimaryKeyColumnName)})";

        Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(UserGroupDto.TableName), columnSql)).Do();
    }
}

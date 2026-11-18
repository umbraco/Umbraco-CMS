// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_10_0;

/// <summary>
///     Adds the column recording the user who scheduled an action to the <c>umbracoContentSchedule</c> table,
///     with a foreign key to the <c>umbracoUser</c> table.
/// </summary>
/// <remarks>
///     Existing schedules aren't attributed to a user, so they are performed on behalf of the last user to save the content, as before.
/// </remarks>
public class AddUserIdColumnToContentSchedule : AsyncMigrationBase
{
    private const string ForeignKeyName = "FK_" + ContentScheduleDto.TableName + "_" + UserDto.TableName + "_" + UserDto.PrimaryKeyColumnName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddUserIdColumnToContentSchedule"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddUserIdColumnToContentSchedule(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (ColumnExists(ContentScheduleDto.TableName, ContentScheduleDto.UserIdColumnName))
        {
            return Task.CompletedTask;
        }

        if (DatabaseType == DatabaseType.SQLite)
        {
            MigrateSqlite();
        }
        else
        {
            MigrateSqlServer();
        }

        return Task.CompletedTask;
    }

    private void MigrateSqlServer()
    {
        AddColumn<ContentScheduleDto>(ContentScheduleDto.TableName, ContentScheduleDto.UserIdColumnName);

        Create.ForeignKey(ForeignKeyName)
            .FromTable(ContentScheduleDto.TableName)
            .ForeignColumn(ContentScheduleDto.UserIdColumnName)
            .ToTable(UserDto.TableName)
            .PrimaryColumn(UserDto.PrimaryKeyColumnName)
            .Do();
    }

    // SQLite can't add a foreign key to an existing table, so the table is recreated from its definition.
    // No other table references it, so it can be dropped without disabling foreign key enforcement.
    private void MigrateSqlite()
    {
        string[] existingColumns =
        [
            ContentScheduleDto.PrimaryKeyColumnName,
            "nodeId",
            "languageId",
            "date",
            "action",
        ];
        List<ContentScheduleDto> schedules = Database.Fetch<ContentScheduleDto>(
            $"SELECT {string.Join(", ", existingColumns.Select(SqlSyntax.GetQuotedColumnName))} FROM {SqlSyntax.GetQuotedTableName(ContentScheduleDto.TableName)}");

        Delete.Table(ContentScheduleDto.TableName).Do();
        Create.Table<ContentScheduleDto>().Do();

        foreach (ContentScheduleDto schedule in schedules)
        {
            Database.Insert(schedule);
        }
    }
}

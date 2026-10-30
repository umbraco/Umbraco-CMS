// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_5_0;

/// <summary>
/// Recreates the default constraints on cmsMember.Email, LoginName and Password as empty strings.
/// </summary>
/// <remarks>
/// The columns were declared with <c>Default = "''"</c>, which the syntax provider quotes again, so the constraint was
/// created as <c>DEFAULT ('''')</c>: a single apostrophe rather than an empty string.
/// </remarks>
public class FixMemberEmptyStringDefaults : AsyncMigrationBase
{
    private static readonly string[] _columnNames = ["Email", "LoginName", "Password"];

    /// <summary>
    /// Initializes a new instance of the <see cref="FixMemberEmptyStringDefaults"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public FixMemberEmptyStringDefaults(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override Task MigrateAsync()
    {
        // SQLite doesn't support altering a column default without rebuilding the table. Members are always saved
        // with explicit values for these columns, so the default is never relied upon and we don't need to migrate it.
        if (DatabaseType == DatabaseType.SQLite)
        {
            return Task.CompletedTask;
        }

        foreach (var columnName in _columnNames)
        {
            RecreateDefaultConstraint(MemberDto.TableName, columnName);
        }

        return Task.CompletedTask;
    }

    private void RecreateDefaultConstraint(string tableName, string columnName)
    {
        var quotedTableName = SqlSyntax.GetQuotedTableName(tableName);
        var quotedColumnName = SqlSyntax.GetQuotedColumnName(columnName);
        var expectedConstraintName = $"DF_{tableName}_{columnName}";

        // A database whose schema was materialised outside of Umbraco can carry a system generated constraint name,
        // or no default constraint at all, so recreate under the expected name in all cases.
        if (SqlSyntax.TryGetDefaultConstraint(Database, tableName, columnName, out var existingConstraintName))
        {
            Database.Execute(new Sql($"ALTER TABLE {quotedTableName} DROP CONSTRAINT {SqlSyntax.GetQuotedName(existingConstraintName)}"));
        }
        else
        {
            Logger.LogInformation(
                "No default constraint was found on {TableName}.{ColumnName}. One will be created named {ExpectedConstraintName}.",
                tableName,
                columnName,
                expectedConstraintName);
        }

        Database.Execute(new Sql($"ALTER TABLE {quotedTableName} ADD CONSTRAINT {SqlSyntax.GetQuotedName(expectedConstraintName)} DEFAULT ('') FOR {quotedColumnName}"));
    }
}

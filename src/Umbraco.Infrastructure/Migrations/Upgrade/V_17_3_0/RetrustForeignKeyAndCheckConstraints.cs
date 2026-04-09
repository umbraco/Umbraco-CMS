using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_3_0;

/// <summary>
/// Re-trusts all untrusted foreign key and check constraints on SQL Server.
/// Untrusted constraints (where <c>is_not_trusted = 1</c>) prevent the query optimizer from using them
/// for join elimination, cardinality estimation, and index selection. They can result from historical
/// upgrades, EF Core migrations, or DBA operations such as backup restores.
/// </summary>
public class RetrustForeignKeyAndCheckConstraints : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetrustForeignKeyAndCheckConstraints"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public RetrustForeignKeyAndCheckConstraints(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (DatabaseType == DatabaseType.SQLite)
        {
            return Task.CompletedTask;
        }

        RetrustConstraints();

        return Task.CompletedTask;
    }

    private void RetrustConstraints()
    {
        // Only target Umbraco tables (prefixed "umbraco" or "cms") to avoid touching
        // custom or third-party tables that share the same database.
        List<UntrustedConstraintDto> untrustedConstraints = Database.Fetch<UntrustedConstraintDto>(
            @"SELECT
                s.name AS SchemaName,
                OBJECT_NAME(fk.parent_object_id) AS TableName,
                fk.name AS ConstraintName
            FROM sys.foreign_keys fk
            INNER JOIN sys.schemas s ON fk.schema_id = s.schema_id
            WHERE fk.is_not_trusted = 1
              AND (OBJECT_NAME(fk.parent_object_id) LIKE 'umbraco%' OR OBJECT_NAME(fk.parent_object_id) LIKE 'cms%')

            UNION ALL

            SELECT
                s.name AS SchemaName,
                OBJECT_NAME(cc.parent_object_id) AS TableName,
                cc.name AS ConstraintName
            FROM sys.check_constraints cc
            INNER JOIN sys.schemas s ON cc.schema_id = s.schema_id
            WHERE cc.is_not_trusted = 1
              AND (OBJECT_NAME(cc.parent_object_id) LIKE 'umbraco%' OR OBJECT_NAME(cc.parent_object_id) LIKE 'cms%')");

        if (untrustedConstraints.Count == 0)
        {
            Logger.LogInformation("No untrusted constraints found.");
            return;
        }

        Logger.LogInformation("Found {Count} untrusted constraint(s) to re-trust.", untrustedConstraints.Count);

        // Ensure we have a long command timeout, in case the migration targets a huge table.
        EnsureLongCommandTimeout(Database);

        var retrusted = 0;
        var failed = 0;

        foreach (UntrustedConstraintDto constraint in untrustedConstraints)
        {
            // Use T-SQL TRY...CATCH to handle errors at the SQL level. This prevents a constraint
            // validation failure from dooming the .NET SqlTransaction, which would cause all
            // subsequent operations to fail with "This SqlTransaction has completed".
            var sql = $@"
BEGIN TRY
    ALTER TABLE [{constraint.SchemaName}].[{constraint.TableName}] WITH CHECK CHECK CONSTRAINT [{constraint.ConstraintName}];
    SELECT CAST(1 AS BIT) AS Success, NULL AS ErrorMessage;
END TRY
BEGIN CATCH
    SELECT CAST(0 AS BIT) AS Success, ERROR_MESSAGE() AS ErrorMessage;
END CATCH";

            RetrustResultDto result = Database.Single<RetrustResultDto>(sql);

            if (result.Success)
            {
                retrusted++;
                Logger.LogDebug(
                    "Re-trusted constraint [{ConstraintName}] on [{SchemaName}].[{TableName}].",
                    constraint.ConstraintName,
                    constraint.SchemaName,
                    constraint.TableName);
            }
            else
            {
                failed++;
                Logger.LogWarning(
                    "Could not re-trust constraint [{ConstraintName}] on [{SchemaName}].[{TableName}]: {ErrorMessage}. " +
                    "This likely means existing data violates the constraint. " +
                    "Please investigate and fix the data integrity issue manually.",
                    constraint.ConstraintName,
                    constraint.SchemaName,
                    constraint.TableName,
                    result.ErrorMessage);
            }
        }

        Logger.LogInformation(
            "Constraint re-trust complete: {Retrusted} succeeded, {Failed} failed out of {Total} total.",
            retrusted,
            failed,
            untrustedConstraints.Count);
    }

    [TableName("")]
    private class UntrustedConstraintDto
    {
        [Column("SchemaName")]
        public string SchemaName { get; set; } = null!;

        [Column("TableName")]
        public string TableName { get; set; } = null!;

        [Column("ConstraintName")]
        public string ConstraintName { get; set; } = null!;
    }

    [TableName("")]
    private class RetrustResultDto
    {
        [Column("Success")]
        public bool Success { get; set; }

        [Column("ErrorMessage")]
        public string? ErrorMessage { get; set; }
    }
}

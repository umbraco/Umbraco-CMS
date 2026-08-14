using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_3_0;

/// <summary>
/// Re-trusts all untrusted foreign key and check constraints on SQL Server.
/// Untrusted constraints (where <c>is_not_trusted = 1</c>) prevent the query optimizer from using them
/// for join elimination, cardinality estimation, and index selection. They can result from historical
/// upgrades, EF Core migrations, or DBA operations such as backup restores.
/// </summary>
public class RetrustForeignKeyAndCheckConstraints : AsyncMigrationBase
{
    private readonly IUmbracoDatabaseFactory _databaseFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetrustForeignKeyAndCheckConstraints"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public RetrustForeignKeyAndCheckConstraints(IMigrationContext context)
        : this(
            context,
            StaticServiceProvider.Instance.GetRequiredService<IUmbracoDatabaseFactory>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetrustForeignKeyAndCheckConstraints"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="databaseFactory">The database factory used to create separate connections for constraint validation.</param>
    public RetrustForeignKeyAndCheckConstraints(IMigrationContext context, IUmbracoDatabaseFactory databaseFactory)
        : base(context) => _databaseFactory = databaseFactory;

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
        List<UntrustedConstraintsQuery.UntrustedConstraintDto> untrustedConstraints =
            Database.Fetch<UntrustedConstraintsQuery.UntrustedConstraintDto>(UntrustedConstraintsQuery.Sql);

        if (untrustedConstraints.Count == 0)
        {
            Logger.LogInformation("No untrusted constraints found.");
            return;
        }

        Logger.LogInformation("Found {Count} untrusted constraint(s) to re-trust.", untrustedConstraints.Count);

        // ALTER TABLE ... WITH CHECK CHECK CONSTRAINT is executed on a separate database connection
        // to isolate failures from the migration scope's transaction. When constraint validation fails
        // (e.g. orphaned FK rows), the error can zombie the .NET SqlTransaction even when caught by
        // T-SQL TRY...CATCH, because the transaction state change propagates through the TDS (Tabular
        // Data Stream) protocol layer that underlies SQL Server client-server communication.
        // Using a separate connection (which has no explicit transaction) avoids this entirely —
        // TRY...CATCH works correctly and no SqlException is thrown to the .NET layer.
        var retrusted = 0;
        var failed = 0;

        using IUmbracoDatabase db = _databaseFactory.CreateDatabase();
        EnsureLongCommandTimeout(db);

        foreach (UntrustedConstraintsQuery.UntrustedConstraintDto constraint in untrustedConstraints)
        {
            // Leading semicolon prevents NPoco's auto-select from prepending
            // "SELECT ... FROM []" based on the empty [TableName("")] attribute.
            var sql = $@";
BEGIN TRY
    ALTER TABLE [{constraint.SchemaName}].[{constraint.TableName}] WITH CHECK CHECK CONSTRAINT [{constraint.ConstraintName}];
    SELECT CAST(1 AS BIT) AS Success, NULL AS ErrorMessage;
END TRY
BEGIN CATCH
    SELECT CAST(0 AS BIT) AS Success, ERROR_MESSAGE() AS ErrorMessage;
END CATCH";

            RetrustResultDto result = db.Single<RetrustResultDto>(sql);

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
    private class RetrustResultDto
    {
        [Column("Success")]
        public bool Success { get; set; }

        [Column("ErrorMessage")]
        public string? ErrorMessage { get; set; }
    }
}

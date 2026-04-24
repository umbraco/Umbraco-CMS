using System.Net;
using System.Text;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.HealthChecks.Checks.Data;

/// <summary>
///     Health check that detects untrusted foreign key and check constraints on SQL Server.
/// </summary>
/// <remarks>
///     An untrusted constraint (<c>is_not_trusted = 1</c>) means SQL Server has not verified that
///     all existing rows satisfy the constraint. This prevents the query optimizer from using the
///     constraint for join elimination and cardinality estimation, and indicates that orphaned or
///     otherwise invalid rows may exist. The <c>RetrustForeignKeyAndCheckConstraints</c> migration
///     attempts to re-trust these constraints automatically but logs a warning and continues when
///     existing data violates a constraint, leaving the issue for manual resolution.
/// </remarks>
[HealthCheck(
    "0B1E71E4-8D37-4F9B-A9A4-86C5B9EA5B0B",
    "Untrusted database constraints",
    Description = "Checks for untrusted foreign key or check constraints on SQL Server, which indicate pre-existing data integrity issues that must be resolved manually.",
    Group = "Data Integrity")]
public class UntrustedDatabaseConstraintsCheck : HealthCheck
{
    private readonly IUmbracoDatabaseFactory _databaseFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UntrustedDatabaseConstraintsCheck" /> class.
    /// </summary>
    /// <param name="databaseFactory">The database factory used to create a connection for the check.</param>
    public UntrustedDatabaseConstraintsCheck(IUmbracoDatabaseFactory databaseFactory)
        => _databaseFactory = databaseFactory;

    /// <inheritdoc />
    public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => Task.FromResult<IEnumerable<HealthCheckStatus>>([CheckConstraints()]);

    private HealthCheckStatus CheckConstraints()
    {
        if (_databaseFactory.Configured is false || _databaseFactory.CanConnect is false)
        {
            return new HealthCheckStatus("Cannot connect to the database.")
            {
                ResultType = StatusResultType.Info,
            };
        }

        using IUmbracoDatabase db = _databaseFactory.CreateDatabase();

#pragma warning disable CS0618 // Type or member is obsolete - justification: in this case we do want to check the database type before running SQL Server-specific queries.
        if (db.DatabaseType.IsSqlServer() is false)
        {
            return new HealthCheckStatus("Not applicable — this check only runs on SQL Server.")
            {
                ResultType = StatusResultType.Info,
            };
        }
#pragma warning restore CS0618 // Type or member is obsolete

        // Only target Umbraco tables (prefixed "umbraco" or "cms") to avoid reporting
        // untrusted constraints on custom or third-party tables that share the database.
        // Mirrors the query used by RetrustForeignKeyAndCheckConstraints migration.
        List<UntrustedConstraintDto> untrustedConstraints = db.Fetch<UntrustedConstraintDto>(
            @"SELECT
                'Foreign key' AS ConstraintType,
                s.name AS SchemaName,
                OBJECT_NAME(fk.parent_object_id) AS TableName,
                fk.name AS ConstraintName
            FROM sys.foreign_keys fk
            INNER JOIN sys.schemas s ON fk.schema_id = s.schema_id
            WHERE fk.is_not_trusted = 1
              AND (OBJECT_NAME(fk.parent_object_id) LIKE 'umbraco%' OR OBJECT_NAME(fk.parent_object_id) LIKE 'cms%')

            UNION ALL

            SELECT
                'Check constraint' AS ConstraintType,
                s.name AS SchemaName,
                OBJECT_NAME(cc.parent_object_id) AS TableName,
                cc.name AS ConstraintName
            FROM sys.check_constraints cc
            INNER JOIN sys.schemas s ON cc.schema_id = s.schema_id
            WHERE cc.is_not_trusted = 1
              AND (OBJECT_NAME(cc.parent_object_id) LIKE 'umbraco%' OR OBJECT_NAME(cc.parent_object_id) LIKE 'cms%')");

        if (untrustedConstraints.Count == 0)
        {
            return new HealthCheckStatus("All Umbraco database constraints are trusted.")
            {
                ResultType = StatusResultType.Success,
            };
        }

        return new HealthCheckStatus(
            $"Found {untrustedConstraints.Count} untrusted constraint(s). These indicate pre-existing data integrity issues that need to be resolved manually.")
        {
            Description = BuildDescription(untrustedConstraints),
            ResultType = StatusResultType.Warning,
            ReadMoreLink = Constants.HealthChecks.DocumentationLinks.Data.UntrustedConstraintsCheck,
        };
    }

    private static string BuildDescription(IReadOnlyList<UntrustedConstraintDto> constraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "<p>Untrusted constraints prevent the SQL Server query optimizer from using them and indicate that orphaned or invalid rows may exist. Resolve the underlying data issue, then re-trust the constraint with <code>ALTER TABLE ... WITH CHECK CHECK CONSTRAINT</code>.</p>");
        sb.AppendLine("<ul>");
        foreach (UntrustedConstraintDto constraint in constraints)
        {
            sb.Append("<li>")
                .Append(WebUtility.HtmlEncode(constraint.ConstraintType))
                .Append(" <code>")
                .Append(WebUtility.HtmlEncode(constraint.ConstraintName))
                .Append("</code> on <code>")
                .Append(WebUtility.HtmlEncode(constraint.SchemaName))
                .Append('.')
                .Append(WebUtility.HtmlEncode(constraint.TableName))
                .AppendLine("</code></li>");
        }

        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    [TableName("")]
    private class UntrustedConstraintDto
    {
        [Column("ConstraintType")]
        public string ConstraintType { get; set; } = null!;

        [Column("SchemaName")]
        public string SchemaName { get; set; } = null!;

        [Column("TableName")]
        public string TableName { get; set; } = null!;

        [Column("ConstraintName")]
        public string ConstraintName { get; set; } = null!;
    }
}

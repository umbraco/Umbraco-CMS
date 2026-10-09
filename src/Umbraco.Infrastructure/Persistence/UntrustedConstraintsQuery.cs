using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Shared SQL and DTO for identifying untrusted foreign key and check constraints on SQL Server.
/// Consumed by the <c>RetrustForeignKeyAndCheckConstraints</c> migration and the
/// <c>UntrustedDatabaseConstraintsCheck</c> health check.
/// </summary>
internal static class UntrustedConstraintsQuery
{
    /// <summary>
    /// Returns one row per untrusted constraint (<c>is_not_trusted = 1</c>) on Umbraco tables
    /// (those prefixed "umbraco" or "cms"), across both foreign keys and check constraints.
    /// Other tables in the same database are deliberately excluded.
    /// </summary>
    public const string Sql = @"SELECT
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
  AND (OBJECT_NAME(cc.parent_object_id) LIKE 'umbraco%' OR OBJECT_NAME(cc.parent_object_id) LIKE 'cms%')";

    /// <summary>
    /// A row returned by <see cref="Sql"/>, describing a single untrusted constraint.
    /// </summary>
    [TableName("")]
    public class UntrustedConstraintDto
    {
        /// <summary>
        /// Gets or sets the kind of constraint — either <c>"Foreign key"</c> or <c>"Check constraint"</c>.
        /// </summary>
        [Column("ConstraintType")]
        public string ConstraintType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the schema name of the table the constraint belongs to (e.g. <c>"dbo"</c>).
        /// </summary>
        [Column("SchemaName")]
        public string SchemaName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the table the constraint belongs to.
        /// </summary>
        [Column("TableName")]
        public string TableName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the untrusted constraint.
        /// </summary>
        [Column("ConstraintName")]
        public string ConstraintName { get; set; } = null!;
    }
}

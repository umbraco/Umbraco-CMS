using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class LogViewerQueryDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.LogViewerQuery;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for this query.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier name for the log viewer query.
    /// </summary>
    [Column("name")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_LogViewerQuery_name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the log viewer query string.
    /// </summary>
    [Column("query")]
    public required string Query { get; set; }
}

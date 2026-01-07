using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName)]
[ExplicitColumns]
internal sealed class LogViewerQueryDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.LogViewerQuery;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("name")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_LogViewerQuery_name")]
    public required string Name { get; set; }

    [Column("query")]
    public required string Query { get; set; }
}

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.LogViewerQuery)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class LogViewerQueryDto
{
    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("name")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_LogViewerQuery_name")]
    public required string Name { get; set; }

    [Column("query")]
    public required string Query { get; set; }
}

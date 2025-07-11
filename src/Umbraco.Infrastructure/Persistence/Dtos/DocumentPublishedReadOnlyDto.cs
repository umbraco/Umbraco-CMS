using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.Document)]
[PrimaryKey("versionId", AutoIncrement = false)]
[ExplicitColumns]
internal class DocumentPublishedReadOnlyDto
{
    [Column("nodeId")]
    public int NodeId { get; set; }

    [Column("published")]
    public bool Published { get; set; }

    [Column("versionId")]
    public Guid VersionId { get; set; }

    [Column("newest")]
    public bool Newest { get; set; }

    [Column("updateDate")]
    public DateTime VersionDate { get; set; }
}

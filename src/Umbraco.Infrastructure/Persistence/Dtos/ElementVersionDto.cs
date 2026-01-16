using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
public sealed class ElementVersionDto : IContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ElementVersion;

    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = "id,published")]
    public int Id { get; set; }

    [Column("published")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = "published", IncludeColumns = "id")]
    public bool Published { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}

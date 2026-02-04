using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
internal sealed class ElementVersionDto : IContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ElementVersion;

    [Column(IContentVersionDto.Columns.Id)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = $"{IContentVersionDto.Columns.Id},{IContentVersionDto.Columns.Published}")]
    public int Id { get; set; }

    [Column(IContentVersionDto.Columns.Published)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = IContentVersionDto.Columns.Published, IncludeColumns = IContentVersionDto.Columns.Id)]
    public bool Published { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}

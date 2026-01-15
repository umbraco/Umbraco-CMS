using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
public class DocumentVersionDto : IContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;

    [Column(IContentVersionDto.IdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = "id,published", IncludeColumns = "templateId")]
    public int Id { get; set; }

    [Column("templateId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(TemplateDto), Column = "nodeId")]
    public int? TemplateId { get; set; }

    [Column(IContentVersionDto.PublishedColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = "published", IncludeColumns = "id,templateId")]
    public bool Published { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}

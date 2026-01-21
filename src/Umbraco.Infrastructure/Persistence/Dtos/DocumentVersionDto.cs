using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(IContentVersionDto.IdColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class DocumentVersionDto : IContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;

    private const string TemplateIdColumnName = "templateId";

    [Column(IContentVersionDto.IdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = $"{IContentVersionDto.PublishedColumnName},{IContentVersionDto.PublishedColumnName}", IncludeColumns = TemplateIdColumnName)]
    public int Id { get; set; }

    [Column(TemplateIdColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(TemplateDto), Column = TemplateDto.NodeIdColumnName)]
    public int? TemplateId { get; set; }

    [Column(IContentVersionDto.PublishedColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = IContentVersionDto.PublishedColumnName, IncludeColumns = $"{IContentVersionDto.IdColumnName},{TemplateIdColumnName}")]
    public bool Published { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}

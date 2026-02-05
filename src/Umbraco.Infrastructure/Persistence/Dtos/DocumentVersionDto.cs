using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(IContentVersionDto.Columns.Id, AutoIncrement = false)]
[ExplicitColumns]
public class DocumentVersionDto : IContentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;

    private const string TemplateIdColumnName = "templateId";

    [Column(IContentVersionDto.Columns.Id)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = $"{IContentVersionDto.Columns.Id},{IContentVersionDto.Columns.Published}", IncludeColumns = TemplateIdColumnName)]
    public int Id { get; set; }

    [Column(TemplateIdColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(TemplateDto), Column = TemplateDto.NodeIdColumnName)]
    public int? TemplateId { get; set; }

    [Column(IContentVersionDto.Columns.Published)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = IContentVersionDto.Columns.Published, IncludeColumns = $"{IContentVersionDto.Columns.Id},{TemplateIdColumnName}")]
    public bool Published { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}

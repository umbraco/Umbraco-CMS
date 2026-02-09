using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class DocumentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string PublishedColumnName = "published";

    private const string TemplateIdColumnName = "templateId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = $"{PrimaryKeyColumnName},{PublishedColumnName}", IncludeColumns = TemplateIdColumnName)]
    public int Id { get; set; }

    [Column(TemplateIdColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(TemplateDto), Column = TemplateDto.NodeIdColumnName)]
    public int? TemplateId { get; set; }

    [Column(PublishedColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = PublishedColumnName, IncludeColumns = $"{PrimaryKeyColumnName},{TemplateIdColumnName}")]
    public bool Published { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
public class DocumentVersionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;
    public const string PrimaryKeyName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string PublishedName = "published";
    public const string TemplateIdName = "templateId";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_id_published", ForColumns = $"{PrimaryKeyName},{PublishedName}", IncludeColumns = TemplateIdName)]
    public int Id { get; set; }

    [Column(TemplateIdName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(TemplateDto), Column = TemplateDto.NodeIdName)]
    public int? TemplateId { get; set; }

    [Column(PublishedName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = PublishedName, IncludeColumns = $"{PrimaryKeyName},{TemplateIdName}")]
    public bool Published { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}

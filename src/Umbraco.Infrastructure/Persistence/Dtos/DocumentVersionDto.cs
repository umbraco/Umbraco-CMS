using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = false)]
[ExplicitColumns]
public class DocumentVersionDto
{
    private const string TableName = Constants.DatabaseSchema.Tables.DocumentVersion;

    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentVersionDto))]
    public int Id { get; set; }

    [Column("templateId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(TemplateDto), Column = "nodeId")]
    public int? TemplateId { get; set; }

    [Column("published")]
    public bool Published { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne)]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}

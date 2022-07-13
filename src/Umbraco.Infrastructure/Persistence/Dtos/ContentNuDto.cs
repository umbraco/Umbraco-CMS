using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.NodeData)]
[PrimaryKey("nodeId", AutoIncrement = false)]
[ExplicitColumns]
public class ContentNuDto
{
    [Column("nodeId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsContentNu", OnColumns = "nodeId, published")]
    [ForeignKey(typeof(ContentDto), Column = "nodeId", OnDelete = Rule.Cascade)]
    public int NodeId { get; set; }

    [Column("published")]
    public bool Published { get; set; }

    /// <summary>
    ///     Stores serialized JSON representing the content item's property and culture name values
    /// </summary>
    /// <remarks>
    ///     Pretty much anything that would require a 1:M lookup is serialized here
    /// </remarks>
    [Column("data")]
    [SpecialDbType(SpecialDbTypes.NTEXT)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Data { get; set; }

    [Column("rv")]
    public long Rv { get; set; }

    [Column("dataRaw")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public byte[]? RawData { get; set; }
}

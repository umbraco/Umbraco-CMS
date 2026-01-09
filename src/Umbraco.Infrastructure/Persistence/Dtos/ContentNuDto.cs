using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = false)]
[ExplicitColumns]
public class ContentNuDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.NodeData;
    public const string PrimaryKeyName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string PublishedName = "published";
    public const string RvName = "rv";

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsContentNu", OnColumns = $"{PrimaryKeyName}, {PublishedName}")]
    [ForeignKey(typeof(ContentDto), Column = PrimaryKeyName, OnDelete = Rule.Cascade)]
    public int NodeId { get; set; }

    [Column(PublishedName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_published", ForColumns = $"{PublishedName},{PrimaryKeyName},{RvName}", IncludeColumns = "dataRaw")]
    public bool Published { get; set; }

    /// <summary>
    ///     Stores serialized JSON representing the content item's property and culture name values
    /// </summary>
    /// <remarks>
    ///     Pretty much anything that would require a 1:M lookup is serialized here
    /// </remarks>
    [Column("data")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Data { get; set; }

    [Column(RvName)]
    public long Rv { get; set; }

    [Column("dataRaw")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public byte[]? RawData { get; set; }
}

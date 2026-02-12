using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
public class ContentNuDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.NodeData;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    private const string PublishedColumnName = "published";
    private const string RvColumnName = "rv";
    private const string DataRawColumnName = "dataRaw";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsContentNu", OnColumns = $"{PrimaryKeyColumnName}, {PublishedColumnName}")]
    [ForeignKey(typeof(ContentDto), Column = PrimaryKeyColumnName, OnDelete = Rule.Cascade)]
    public int NodeId { get; set; }

    [Column(PublishedColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_" + PublishedColumnName, ForColumns = $"{PublishedColumnName},{PrimaryKeyColumnName},{RvColumnName}", IncludeColumns = DataRawColumnName)]
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

    [Column(RvColumnName)]
    public long Rv { get; set; }

    [Column(DataRawColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public byte[]? RawData { get; set; }
}

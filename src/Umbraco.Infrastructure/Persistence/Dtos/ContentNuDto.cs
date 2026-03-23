using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) for content entities used in the Umbraco CMS persistence layer.
/// This class is typically used for database operations involving content data.
/// </summary>
[TableName(TableName)]
[PrimaryKey([NodeIdColumnName, PublishedColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class ContentNuDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.NodeData;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    [Obsolete("Use NodeIdColumnName instead. Scheduled for removal in Umbraco 18.")]
    public const string PrimaryKeyColumnName = NodeIdColumnName;

    private const string PublishedColumnName = "published";
    private const string RvColumnName = "rv";
    private const string DataRawColumnName = "dataRaw";

    /// <summary>
    /// Gets or sets the unique identifier of the content node.
    /// </summary>
    [Column(NodeIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_cmsContentNu", OnColumns = $"{NodeIdColumnName}, {PublishedColumnName}")]
    [ForeignKey(typeof(ContentDto), Column = ContentDto.PrimaryKeyColumnName, OnDelete = Rule.Cascade)]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the content is published.
    /// </summary>
    [Column(PublishedColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_" + PublishedColumnName, ForColumns = $"{PublishedColumnName},{NodeIdColumnName},{RvColumnName}", IncludeColumns = DataRawColumnName)]
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

    /// <summary>
    /// Gets or sets the revision number (Rv), which represents the version of the content in the database.
    /// </summary>
    [Column(RvColumnName)]
    public long Rv { get; set; }

    /// <summary>
    /// Gets or sets the raw binary data representing the serialized state of the content item.
    /// This data is typically used for caching or persistence purposes within the Umbraco CMS.
    /// </summary>
    [Column(DataRawColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public byte[]? RawData { get; set; }
}

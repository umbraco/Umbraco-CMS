using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal class ContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    private string? _alias;

    // Public constants to bind properties between DTOs
    public const string VariationsColumnName = "variations";

    /// <summary>
    /// Gets or sets the primary key of the content type.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = 700)]
    public int PrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the node associated with this content type.
    /// </summary>
    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsContentType")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the alias that uniquely identifies the content type.
    /// </summary>
    [Column("alias")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Alias { get => _alias; set => _alias = value == null ? null : string.Intern(value); }

    /// <summary>
    /// Gets or sets the icon associated with the content type, typically as a string representing a CSS class or image identifier.
    /// </summary>
    [Column("icon")]
    [Index(IndexTypes.NonClustered)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail image file name associated with the content type.
    /// </summary>
    [Column("thumbnail")]
    [Constraint(Default = "folder.png")]
    public string? Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the description of the content type.
    /// </summary>
    [Column("description")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(1500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the list view configuration associated with this content type, if any.
    /// </summary>
    [Column("listView")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? ListView { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this content type is an element.
    /// </summary>
    [Column("isElement")]
    [Constraint(Default = "0")]
    public bool IsElement { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this content type is allowed at the root level.
    /// </summary>
    [Column("allowAtRoot")]
    [Constraint(Default = "0")]
    public bool AllowAtRoot { get; set; }

    /// <summary>
    /// Gets or sets the variation flags for the content type, indicating how the content type supports culture and segment variations.
    /// </summary>
    [Column(VariationsColumnName)]
    [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
    public byte Variations { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.NodeDto"/> associated with this content type.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = nameof(NodeId))]
    public NodeDto NodeDto { get; set; } = null!;
}

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

    internal const string ReferenceColumnName = "NodeId"; // should be ContentTypeDto.NodeIdColumnName, but for database compatibility we keep it like this

    private string? _alias;

    // Public constants to bind properties between DTOs
    public const string VariationsColumnName = "variations";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = 700)]
    public int PrimaryKey { get; set; }

    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsContentType")]
    public int NodeId { get; set; }

    [Column("alias")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Alias { get => _alias; set => _alias = value == null ? null : string.Intern(value); }

    [Column("icon")]
    [Index(IndexTypes.NonClustered)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Icon { get; set; }

    [Column("thumbnail")]
    [Constraint(Default = "folder.png")]
    public string? Thumbnail { get; set; }

    [Column("description")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(1500)]
    public string? Description { get; set; }

    [Column("listView")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? ListView { get; set; }

    [Column("isElement")]
    [Constraint(Default = "0")]
    public bool IsElement { get; set; }

    [Column("allowAtRoot")]
    [Constraint(Default = "0")]
    public bool AllowAtRoot { get; set; }

    [Column(VariationsColumnName)]
    [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
    public byte Variations { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ColumnName = ReferenceColumnName)]
    public NodeDto NodeDto { get; set; } = null!;
}

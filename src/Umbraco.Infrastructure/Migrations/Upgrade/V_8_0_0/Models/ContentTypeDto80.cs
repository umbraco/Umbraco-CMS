using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.Models;

/// <summary>
///     Snapshot of the <see cref="ContentTypeDto" /> as it was at version 8.0
/// </summary>
/// <remarks>
///     This is required during migrations the schema of this table changed and running SQL against the new table would
///     result in errors
/// </remarks>
[TableName(TableName)]
[PrimaryKey("pk")]
[ExplicitColumns]
internal class ContentTypeDto80
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentType;

    [Column("pk")]
    [PrimaryKeyColumn(IdentitySeed = 535)]
    public int PrimaryKey { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsContentType")]
    public int NodeId { get; set; }

    [Column("alias")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Alias { get; set; }

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

    [Column("isContainer")]
    [Constraint(Default = "0")]
    public bool IsContainer { get; set; }

    [Column("allowAtRoot")]
    [Constraint(Default = "0")]
    public bool AllowAtRoot { get; set; }

    [Column("variations")]
    [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
    public byte Variations { get; set; }

    [ResultColumn]
    public NodeDto? NodeDto { get; set; }
}

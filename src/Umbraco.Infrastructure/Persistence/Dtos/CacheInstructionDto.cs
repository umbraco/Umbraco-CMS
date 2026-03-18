using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Data transfer object representing a cache instruction for persistence operations.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class CacheInstructionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.CacheInstruction;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique ID of the cache instruction.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [PrimaryKeyColumn(AutoIncrement = true, Name = "PK_umbracoCacheInstruction")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp for the cache instruction.
    /// </summary>
    [Column("utcStamp")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime UtcStamp { get; set; }

    /// <summary>
    /// Gets or sets the JSON-serialized instructions used for cache operations.
    /// </summary>
    [Column("jsonInstruction")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Instructions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the origin that issued the cache instruction.
    /// </summary>
    [Column("originated")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Length(500)]
    public string OriginIdentity { get; set; } = null!;

    /// <summary>
    /// Gets or sets the count of instructions.
    /// </summary>
    [Column("instructionCount")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = 1)]
    public int InstructionCount { get; set; }
}

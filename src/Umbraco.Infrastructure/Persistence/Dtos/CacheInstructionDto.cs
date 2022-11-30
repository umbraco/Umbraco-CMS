using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.CacheInstruction)]
[PrimaryKey("id")]
[ExplicitColumns]
public class CacheInstructionDto
{
    [Column("id")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [PrimaryKeyColumn(AutoIncrement = true, Name = "PK_umbracoCacheInstruction")]
    public int Id { get; set; }

    [Column("utcStamp")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime UtcStamp { get; set; }

    [Column("jsonInstruction")]
    [SpecialDbType(SpecialDbTypes.NTEXT)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Instructions { get; set; } = null!;

    [Column("originated")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Length(500)]
    public string OriginIdentity { get; set; } = null!;

    [Column("instructionCount")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = 1)]
    public int InstructionCount { get; set; }
}

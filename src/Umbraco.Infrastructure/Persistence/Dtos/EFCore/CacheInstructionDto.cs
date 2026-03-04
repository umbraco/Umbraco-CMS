using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(CacheInstructionDtoConfiguration))]
public class CacheInstructionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.CacheInstruction;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    public int Id { get; set; }

    public DateTime UtcStamp { get; set; }

    public string Instructions { get; set; } = null!;

    public string OriginIdentity { get; set; } = null!;

    public int InstructionCount { get; set; }
}

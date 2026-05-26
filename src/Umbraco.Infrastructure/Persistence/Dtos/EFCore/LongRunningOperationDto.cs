using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(LongRunningOperationDtoConfiguration))]
public class LongRunningOperationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.LongRunningOperation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    // Public constants to bind properties between configurations and customizers.
    public const string IdColumnName = PrimaryKeyColumnName;
    public const string TypeColumnName = "type";
    public const string StatusColumnName = "status";
    public const string ResultColumnName = "result";
    public const string CreateDateColumnName = "createDate";
    public const string UpdateDateColumnName = "updateDate";
    public const string ExpirationDateColumnName = "expirationDate";

    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Result { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

    public DateTime ExpirationDate { get; set; }
}

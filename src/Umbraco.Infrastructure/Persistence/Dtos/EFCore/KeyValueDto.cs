using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(KeyValueDtoConfiguration))]
public class KeyValueDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.KeyValue;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameKey;

    public required string Key { get; set; }

    public string? Value { get; set; }

    public DateTime UpdateDate { get; set; }
}

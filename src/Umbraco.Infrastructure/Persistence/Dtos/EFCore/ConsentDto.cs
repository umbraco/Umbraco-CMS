using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ConsentDtoConfiguration))]
public class ConsentDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Consent;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    public int Id { get; set; }

    public bool Current { get; set; }

    public string? Source { get; set; }

    public string? Context { get; set; }

    public string? Action { get; set; }

    public DateTime CreateDate { get; set; }

    public int State { get; set; }

    public string? Comment { get; set; }
}

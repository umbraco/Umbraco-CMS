using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DomainDtoConfiguration))]
public class DomainDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Domain;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    public int Id { get; set; }

    public Guid Key { get; set; }

    public int? DefaultLanguage { get; set; }

    public int? RootStructureId { get; set; }

    public string DomainName { get; set; } = null!;

    public int SortOrder { get; set; }
}

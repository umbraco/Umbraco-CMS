using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Represents a mapper that defines the mapping configuration for the <c>Domain</c> entity between the database and the domain model.
/// </summary>
[MapperFor(typeof(IDomain))]
[MapperFor(typeof(UmbracoDomain))]
public sealed class DomainMapper : BaseMapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainMapper"/> class.
    /// </summary>
    /// <param name="sqlContext">The lazy-loaded SQL context used for database operations.</param>
    /// <param name="maps">The configuration store containing mapping definitions.</param>
    public DomainMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.Id), nameof(DomainDto.Id));
        DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.RootContentId), nameof(DomainDto.RootStructureId));
        DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.LanguageId), nameof(DomainDto.DefaultLanguage));
        DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.DomainName), nameof(DomainDto.DomainName));
        DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.SortOrder), nameof(DomainDto.SortOrder));
    }
}

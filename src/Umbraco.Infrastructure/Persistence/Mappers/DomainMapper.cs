using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

[MapperFor(typeof(IDomain))]
[MapperFor(typeof(UmbracoDomain))]
public sealed class DomainMapper : BaseMapper
{
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
    }
}

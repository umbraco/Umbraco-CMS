using System;
using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Infrastructure.Persistence.Mappers;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(IDomain))]
    [MapperFor(typeof(UmbracoDomain))]
    public sealed class DomainMapper : BaseMapper
    {
        public DomainMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.Id), nameof(DomainDto.Id));
            DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.RootContentId), nameof(DomainDto.RootStructureId));
            DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.LanguageId), nameof(DomainDto.DefaultLanguage));
            DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.DomainName), nameof(DomainDto.DomainName));
        }
    }
}

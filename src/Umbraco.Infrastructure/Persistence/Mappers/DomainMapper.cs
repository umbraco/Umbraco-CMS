using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(IDomain))]
    [MapperFor(typeof(UmbracoDomain))]
    public sealed class DomainMapper : BaseMapper
    {
        public DomainMapper(Lazy<ISqlContext> sqlContext)
            : base(sqlContext)
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

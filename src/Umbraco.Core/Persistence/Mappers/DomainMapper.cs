using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(IDomain))]
    [MapperFor(typeof(UmbracoDomain))]
    public sealed class DomainMapper : BaseMapper
    {
        public DomainMapper(ISqlContext sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        {
            DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.Id), nameof(DomainDto.Id));
            DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.RootContentId), nameof(DomainDto.RootStructureId));
            DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.LanguageId), nameof(DomainDto.DefaultLanguage));
            DefineMap<UmbracoDomain, DomainDto>(nameof(UmbracoDomain.DomainName), nameof(DomainDto.DomainName));
        }
    }
}

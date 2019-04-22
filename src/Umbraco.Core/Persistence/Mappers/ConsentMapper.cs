﻿using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a mapper for consent entities.
    /// </summary>
    [MapperFor(typeof(IConsent))]
    [MapperFor(typeof(Consent))]
    public sealed class ConsentMapper : BaseMapper
    {
        public ConsentMapper(Lazy<ISqlContext> sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        { }

        protected override void DefineMaps()
        {
            DefineMap<Consent, ConsentDto>(nameof(Consent.Id), nameof(ConsentDto.Id));
            DefineMap<Consent, ConsentDto>(nameof(Consent.Current), nameof(ConsentDto.Current));
            DefineMap<Consent, ConsentDto>(nameof(Consent.CreateDate), nameof(ConsentDto.CreateDate));
            DefineMap<Consent, ConsentDto>(nameof(Consent.Source), nameof(ConsentDto.Source));
            DefineMap<Consent, ConsentDto>(nameof(Consent.Context), nameof(ConsentDto.Context));
            DefineMap<Consent, ConsentDto>(nameof(Consent.Action), nameof(ConsentDto.Action));
            DefineMap<Consent, ConsentDto>(nameof(Consent.State), nameof(ConsentDto.State));
            DefineMap<Consent, ConsentDto>(nameof(Consent.Comment), nameof(ConsentDto.Comment));
        }
    }
}

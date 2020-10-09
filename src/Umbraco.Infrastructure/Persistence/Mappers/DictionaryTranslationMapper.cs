﻿using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="DictionaryTranslation"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(DictionaryTranslation))]
    [MapperFor(typeof(IDictionaryTranslation))]
    public sealed class DictionaryTranslationMapper : BaseMapper
    {
        public DictionaryTranslationMapper(Lazy<ISqlContext> sqlContext)
            : base(sqlContext)
        { }

        protected override void DefineMaps()
        {
            DefineMap<DictionaryTranslation, LanguageTextDto>(nameof(DictionaryTranslation.Id), nameof(LanguageTextDto.PrimaryKey));
            DefineMap<DictionaryTranslation, LanguageTextDto>(nameof(DictionaryTranslation.Key), nameof(LanguageTextDto.UniqueId));
            DefineMap<DictionaryTranslation, LanguageTextDto>(nameof(DictionaryTranslation.Language), nameof(LanguageTextDto.LanguageId));
            DefineMap<DictionaryTranslation, LanguageTextDto>(nameof(DictionaryTranslation.Value), nameof(LanguageTextDto.Value));
        }
    }
}

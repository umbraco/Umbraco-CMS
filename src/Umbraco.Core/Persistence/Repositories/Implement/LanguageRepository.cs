using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="Language"/>
    /// </summary>
    internal class LanguageRepository : NPocoRepositoryBase<int, ILanguage>, ILanguageRepository
    {
        private readonly Dictionary<string, int> _codeIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, string> _idCodeMap = new Dictionary<int, string>();

        public LanguageRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override IRepositoryCachePolicy<ILanguage, int> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<ILanguage, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ false);
        }

        private FullDataSetRepositoryCachePolicy<ILanguage, int> TypedCachePolicy => CachePolicy as FullDataSetRepositoryCachePolicy<ILanguage, int>;

        #region Overrides of RepositoryBase<int,Language>

        protected override ILanguage PerformGet(int id)
        {
            throw new NotSupportedException(); // not required since policy is full dataset
        }

        protected override IEnumerable<ILanguage> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false).Where<LanguageDto>(x => x.Id > 0);
            if (ids.Any())
            {
                sql.WhereIn<LanguageDto>(x => x.Id, ids);
            }

            //this needs to be sorted since that is the way legacy worked - default language is the first one!!
            //even though legacy didn't sort, it should be by id
            sql.OrderBy<LanguageDto>(x => x.Id);

            // get languages
            var languages = Database.Fetch<LanguageDto>(sql).Select(ConvertFromDto).OrderBy(x => x.Id).ToList();

            // initialize the code-id map
            lock (_codeIdMap)
            {
                _codeIdMap.Clear();
                _idCodeMap.Clear();
                foreach (var language in languages)
                {
                    _codeIdMap[language.IsoCode] = language.Id;
                    _idCodeMap[language.Id] = language.IsoCode.ToLowerInvariant();
                }
            }

            return languages;
        }

        protected override IEnumerable<ILanguage> PerformGetByQuery(IQuery<ILanguage> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ILanguage>(sqlClause, query);
            var sql = translator.Translate();
            var dtos = Database.Fetch<LanguageDto>(sql);
            return dtos.Select(ConvertFromDto).ToList();
        }

        #endregion

        #region Overrides of NPocoRepositoryBase<int,Language>

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<LanguageDto>();

            sql.From<LanguageDto>();

            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return $"{Constants.DatabaseSchema.Tables.Language}.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               //NOTE: There is no constraint between the Language and cmsDictionary/cmsLanguageText tables (?)
                               // but we still need to remove them
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.DictionaryValue + " WHERE languageId = @id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE languageId = @id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersionCultureVariation + " WHERE languageId = @id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.DocumentCultureVariation + " WHERE languageId = @id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.TagRelationship + " WHERE tagId IN (SELECT id FROM " + Constants.DatabaseSchema.Tables.Tag + " WHERE languageId = @id)",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.Tag + " WHERE languageId = @id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.Language + " WHERE id = @id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId => throw new NotImplementedException();

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(ILanguage entity)
        {
            // validate iso code and culture name
            if (entity.IsoCode.IsNullOrWhiteSpace() || entity.CultureName.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Cannot save a language without an ISO code and a culture name.");

            entity.AddingEntity();

            // deal with entity becoming the new default entity
            if (entity.IsDefault)
            {
                // set all other entities to non-default
                // safe (no race cond) because the service locks languages
                var setAllDefaultToFalse = Sql()
                    .Update<LanguageDto>(u => u.Set(x => x.IsDefault, false));
                Database.Execute(setAllDefaultToFalse);
            }

            // fallback cycles are detected at service level

            // insert
            var dto = LanguageFactory.BuildDto(entity);
            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;
            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ILanguage entity)
        {
            // validate iso code and culture name
            if (entity.IsoCode.IsNullOrWhiteSpace() || entity.CultureName.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Cannot save a language without an ISO code and a culture name.");

            entity.UpdatingEntity();

            if (entity.IsDefault)
            {
                // deal with entity becoming the new default entity

                // set all other entities to non-default
                // safe (no race cond) because the service locks languages
                var setAllDefaultToFalse = Sql()
                    .Update<LanguageDto>(u => u.Set(x => x.IsDefault, false));
                Database.Execute(setAllDefaultToFalse);
            }
            else
            {
                // deal with the entity not being default anymore
                // which is illegal - another entity has to become default
                var selectDefaultId = Sql()
                    .Select<LanguageDto>(x => x.Id)
                    .From<LanguageDto>()
                    .Where<LanguageDto>(x => x.IsDefault);

                var defaultId = Database.ExecuteScalar<int>(selectDefaultId);
                if (entity.Id == defaultId)
                    throw new InvalidOperationException($"Cannot save the default language ({entity.IsoCode}) as non-default. Make another language the default language instead.");
            }

            if (entity.IsPropertyDirty(nameof(ILanguage.IsoCode)))
            {
                //if the iso code is changing, ensure there's not another lang with the same code already assigned
                var sameCode = Sql()
                   .SelectCount()                   
                   .From<LanguageDto>()
                   .Where<LanguageDto>(x => x.IsoCode == entity.IsoCode && x.Id != entity.Id);

                var countOfSameCode = Database.ExecuteScalar<int>(sameCode);
                if (countOfSameCode > 0)
                    throw new InvalidOperationException($"Cannot update the language to a new culture: {entity.IsoCode} since that culture is already assigned to another language entity.");
            }

            // fallback cycles are detected at service level

            // update
            var dto = LanguageFactory.BuildDto(entity);
            Database.Update(dto);
            entity.ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(ILanguage entity)
        {
            // validate that the entity is not the default language.
            // safe (no race cond) because the service locks languages

            var selectDefaultId = Sql()
                .Select<LanguageDto>(x => x.Id)
                .From<LanguageDto>()
                .Where<LanguageDto>(x => x.IsDefault);

            var defaultId = Database.ExecuteScalar<int>(selectDefaultId);
            if (entity.Id == defaultId)
                throw new InvalidOperationException($"Cannot delete the default language ({entity.IsoCode}).");

            // We need to remove any references to the language if it's being used as a fall-back from other ones
            var clearFallbackLanguage = Sql()
                .Update<LanguageDto>(u => u
                    .Set(x => x.FallbackLanguageId, null))
                .Where<LanguageDto>(x => x.FallbackLanguageId == entity.Id);

            Database.Execute(clearFallbackLanguage);

            // delete
            base.PersistDeletedItem(entity);
        }

        #endregion

        protected ILanguage ConvertFromDto(LanguageDto dto)
        {
            var entity = LanguageFactory.BuildEntity(dto);
            return entity;
        }

        public ILanguage GetByIsoCode(string isoCode)
        {
            // ensure cache is populated, in a non-expensive way
            if (TypedCachePolicy != null)
                TypedCachePolicy.GetAllCached(PerformGetAll);


            var id = GetIdByIsoCode(isoCode, throwOnNotFound: false);
            return id.HasValue ? Get(id.Value) : null;
        }

        // fast way of getting an id for an isoCode - avoiding cloning
        // _codeIdMap is rebuilt whenever PerformGetAll runs
        public int? GetIdByIsoCode(string isoCode, bool throwOnNotFound = true)
        {
            if (isoCode == null) return null;

            // ensure cache is populated, in a non-expensive way
            if (TypedCachePolicy != null)
                TypedCachePolicy.GetAllCached(PerformGetAll);
            else
                PerformGetAll(); //we don't have a typed cache (i.e. unit tests) but need to populate the _codeIdMap

            lock (_codeIdMap)
            {
                if (_codeIdMap.TryGetValue(isoCode, out var id)) return id;
            }
            if (throwOnNotFound)
                throw new ArgumentException($"Code {isoCode} does not correspond to an existing language.", nameof(isoCode));

            return null;
        }
        
        // fast way of getting an isoCode for an id - avoiding cloning
        // _idCodeMap is rebuilt whenever PerformGetAll runs
        public string GetIsoCodeById(int? id, bool throwOnNotFound = true)
        {
            if (id == null) return null;

            // ensure cache is populated, in a non-expensive way
            if (TypedCachePolicy != null)
                TypedCachePolicy.GetAllCached(PerformGetAll);
            else
                PerformGetAll();

            lock (_codeIdMap) // yes, we want to lock _codeIdMap
            {
                if (_idCodeMap.TryGetValue(id.Value, out var isoCode)) return isoCode;
            }
            if (throwOnNotFound)
                throw new ArgumentException($"Id {id} does not correspond to an existing language.", nameof(id));

            return null;
        }

        public string GetDefaultIsoCode()
        {
            return GetDefault()?.IsoCode;
        }

        public int? GetDefaultId()
        {
            return GetDefault()?.Id;
        }

        // do NOT leak that language, it's not deep-cloned!
        private ILanguage GetDefault()
        {
            // get all cached
            var languages = (TypedCachePolicy?.GetAllCached(PerformGetAll) //try to get all cached non-cloned if using the correct cache policy (not the case in unit tests)
                ?? CachePolicy.GetAll(Array.Empty<int>(), PerformGetAll)).ToList();

            var language = languages.FirstOrDefault(x => x.IsDefault);
            if (language != null) return language;

            // this is an anomaly, the service/repo should ensure it cannot happen
            Logger.Warn<LanguageRepository>("There is no default language. Fix this anomaly by editing the language table in database and setting one language as the default language.");

            // still, don't kill the site, and return "something"

            ILanguage first = null;
            foreach (var l in languages)
            {
                if (first == null || l.Id < first.Id)
                    first = l;
            }

            return first;
        }
    }
}

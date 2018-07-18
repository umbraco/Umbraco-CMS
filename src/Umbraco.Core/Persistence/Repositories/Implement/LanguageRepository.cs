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

        public LanguageRepository(IScopeAccessor scopeAccessor, CacheHelper cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override IRepositoryCachePolicy<ILanguage, int> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<ILanguage, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ false);
        }

        private FullDataSetRepositoryCachePolicy<ILanguage, int> TypedCachePolicy => (FullDataSetRepositoryCachePolicy<ILanguage, int>) CachePolicy;

        #region Overrides of RepositoryBase<int,Language>

        protected override ILanguage PerformGet(int id)
        {
            throw new NotSupportedException(); // not required since policy is full dataset
        }

        protected override IEnumerable<ILanguage> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false).Where("umbracoLanguage.id > 0");
            if (ids.Any())
            {
                sql.Where("umbracoLanguage.id in (@ids)", new { ids = ids });
            }

            //this needs to be sorted since that is the way legacy worked - default language is the first one!!
            //even though legacy didn't sort, it should be by id
            sql.OrderBy<LanguageDto>(dto => dto.Id);

            // get languages
            var dtos = Database.Fetch<LanguageDto>(sql);
            var languages = dtos.Select(ConvertFromDto).ToList();

            // fix inconsistencies: there has to be a default language, and it has to be mandatory
            var defaultLanguage = languages.FirstOrDefault(x => x.IsDefaultVariantLanguage) ?? languages.First();
            defaultLanguage.IsDefaultVariantLanguage = true;
            defaultLanguage.Mandatory = true;

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

            sql
               .From<LanguageDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoLanguage.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {

            var list = new List<string>
                           {
                               //NOTE: There is no constraint between the Language and cmsDictionary/cmsLanguageText tables (?)
                               // but we still need to remove them
                               "DELETE FROM cmsLanguageText WHERE languageId = @id",
                               "DELETE FROM umbracoPropertyData WHERE languageId = @id",
                               "DELETE FROM umbracoContentVersionCultureVariation WHERE languageId = @id",
                               "DELETE FROM umbracoDocumentCultureVariation WHERE languageId = @id",
                               "DELETE FROM umbracoLanguage WHERE id = @id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(ILanguage entity)
        {
            if (entity.IsoCode.IsNullOrWhiteSpace() || entity.CultureInfo == null || entity.CultureName.IsNullOrWhiteSpace())
                throw new InvalidOperationException("The required language data is missing");

            ((EntityBase)entity).AddingEntity();

            if (entity.IsDefaultVariantLanguage)
            {
                //if this entity is flagged as the default, we need to set all others to false
                Database.Execute(Sql().Update<LanguageDto>(u => u.Set(x => x.IsDefaultVariantLanguage, false)));
                //We need to clear the whole cache since all languages will be updated
                IsolatedCache.ClearAllCache();
            }

            var dto = LanguageFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ILanguage entity)
        {
            if (entity.IsoCode.IsNullOrWhiteSpace() || entity.CultureInfo == null || entity.CultureName.IsNullOrWhiteSpace())
                throw new InvalidOperationException("The required language data is missing");

            ((EntityBase)entity).UpdatingEntity();

            if (entity.IsDefaultVariantLanguage)
            {
                //if this entity is flagged as the default, we need to set all others to false
                Database.Execute(Sql().Update<LanguageDto>(u => u.Set(x => x.IsDefaultVariantLanguage, false)));
                //We need to clear the whole cache since all languages will be updated
                IsolatedCache.ClearAllCache();
            }
            
            var dto = LanguageFactory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();

            //Clear the cache entries that exist by key/iso
            IsolatedCache.ClearCacheItem(RepositoryCacheKeys.GetKey<ILanguage>(entity.IsoCode));
            IsolatedCache.ClearCacheItem(RepositoryCacheKeys.GetKey<ILanguage>(entity.CultureName));
        }

        protected override void PersistDeletedItem(ILanguage entity)
        {
            //we need to validate that we can delete this language
            if (entity.IsDefaultVariantLanguage)
                throw new InvalidOperationException($"Cannot delete the default language ({entity.IsoCode})");

            var count = Database.ExecuteScalar<int>(Sql().SelectCount().From<LanguageDto>());
            if (count == 1)
                throw new InvalidOperationException($"Cannot delete the default language ({entity.IsoCode})");

            base.PersistDeletedItem(entity);

            //Clear the cache entries that exist by key/iso
            IsolatedCache.ClearCacheItem(RepositoryCacheKeys.GetKey<ILanguage>(entity.IsoCode));
            IsolatedCache.ClearCacheItem(RepositoryCacheKeys.GetKey<ILanguage>(entity.CultureName));
        }

        #endregion

        protected ILanguage ConvertFromDto(LanguageDto dto)
        {
            var entity = LanguageFactory.BuildEntity(dto);
            return entity;
        }

        public ILanguage GetByIsoCode(string isoCode)
        {
            TypedCachePolicy.GetAllCached(PerformGetAll); // ensure cache is populated, in a non-expensive way
            var id = GetIdByIsoCode(isoCode, throwOnNotFound: false);
            return id.HasValue ? Get(id.Value) : null;
        }

        // fast way of getting an id for an isoCode - avoiding cloning
        // _codeIdMap is rebuilt whenever PerformGetAll runs
        public int? GetIdByIsoCode(string isoCode) => GetIdByIsoCode(isoCode, throwOnNotFound: true);

        private int? GetIdByIsoCode(string isoCode, bool throwOnNotFound)
        {
            if (isoCode == null) return null;

            TypedCachePolicy.GetAllCached(PerformGetAll); // ensure cache is populated, in a non-expensive way
            lock (_codeIdMap)
            {
                if (_codeIdMap.TryGetValue(isoCode, out var id)) return id;
            }
            if (throwOnNotFound)
                throw new ArgumentException($"Code {isoCode} does not correspond to an existing language.", nameof(isoCode));
            return 0;
        }

        // fast way of getting an isoCode for an id - avoiding cloning
        // _idCodeMap is rebuilt whenever PerformGetAll runs
        public string GetIsoCodeById(int? id) => GetIsoCodeById(id, throwOnNotFound: true);

        private string GetIsoCodeById(int? id, bool throwOnNotFound)
        {
            if (id == null) return null;

            TypedCachePolicy.GetAllCached(PerformGetAll); // ensure cache is populated, in a non-expensive way
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
            // get all cached, non-cloned
            var all = TypedCachePolicy.GetAllCached(PerformGetAll);

            ILanguage first = null;
            foreach (var language in all)
            {
                // if one language is default, return
                if (language.IsDefaultVariantLanguage)
                    return language;
                // keep track of language with lowest id
                if (first == null || language.Id < first.Id)
                    first = language;
            }

            return first;
        }
    }
}

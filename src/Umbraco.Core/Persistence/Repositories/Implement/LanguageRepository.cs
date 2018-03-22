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
        public LanguageRepository(IScopeAccessor scopeAccessor, CacheHelper cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override IRepositoryCachePolicy<ILanguage, int> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<ILanguage, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ false);
        }

        #region Overrides of RepositoryBase<int,Language>

        protected override ILanguage PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all domains
            return GetMany().FirstOrDefault(x => x.Id == id);
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


            return Database.Fetch<LanguageDto>(sql).Select(ConvertFromDto);
        }

        protected override IEnumerable<ILanguage> PerformGetByQuery(IQuery<ILanguage> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ILanguage>(sqlClause, query);
            var sql = translator.Translate();
            return Database.Fetch<LanguageDto>(sql).Select(ConvertFromDto);
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
            ((EntityBase)entity).AddingEntity();

            if (entity.IsDefaultVariantLanguage)
            {
                //if this entity is flagged as the default, we need to set all others to false
                Database.Execute($"UPDATE {SqlSyntax.GetQuotedColumnName(Constants.DatabaseSchema.Tables.Language)} SET {SqlSyntax.GetQuotedColumnName("isDefaultVariantLang")} = 0");
                //We need to clear the whole cache since all languages will be updated
                IsolatedCache.ClearAllCache();
            }

            var factory = new LanguageFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();

        }

        protected override void PersistUpdatedItem(ILanguage entity)
        {
            ((EntityBase)entity).UpdatingEntity();

            if (entity.IsDefaultVariantLanguage)
            {
                //if this entity is flagged as the default, we need to set all others to false
                Database.Execute($"UPDATE {SqlSyntax.GetQuotedColumnName(Constants.DatabaseSchema.Tables.Language)} SET {SqlSyntax.GetQuotedColumnName("isDefaultVariantLang")} = 0");
                //We need to clear the whole cache since all languages will be updated
                IsolatedCache.ClearAllCache();
            }

            var factory = new LanguageFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();

            //Clear the cache entries that exist by key/iso
            IsolatedCache.ClearCacheItem(RepositoryCacheKeys.GetKey<ILanguage>(entity.IsoCode));
            IsolatedCache.ClearCacheItem(RepositoryCacheKeys.GetKey<ILanguage>(entity.CultureName));
        }

        protected override void PersistDeletedItem(ILanguage entity)
        {
            base.PersistDeletedItem(entity);

            //Clear the cache entries that exist by key/iso
            IsolatedCache.ClearCacheItem(RepositoryCacheKeys.GetKey<ILanguage>(entity.IsoCode));
            IsolatedCache.ClearCacheItem(RepositoryCacheKeys.GetKey<ILanguage>(entity.CultureName));
        }

        #endregion

        protected ILanguage ConvertFromDto(LanguageDto dto)
        {
            var factory = new LanguageFactory();
            var entity = factory.BuildEntity(dto);
            return entity;
        }

        public ILanguage GetByCultureName(string cultureName)
        {
            //use the underlying GetAll which will force cache all languages
            return GetMany().FirstOrDefault(x => x.CultureName.InvariantEquals(cultureName));
        }

        public ILanguage GetByIsoCode(string isoCode)
        {
            //use the underlying GetAll which will force cache all languages
            return GetMany().FirstOrDefault(x => x.IsoCode.InvariantEquals(isoCode));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="Language"/>
    /// </summary>
    internal class LanguageRepository : PetaPocoRepositoryBase<int, ILanguage>, ILanguageRepository
    {
        public LanguageRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
            //Custom cache options for better performance
            _cacheOptions = new RepositoryCacheOptions
            {
                GetAllCacheAllowZeroCount = true,
                GetAllCacheValidateCount = false
            };
        }

        private readonly RepositoryCacheOptions _cacheOptions;

        /// <summary>
        /// Returns the repository cache options
        /// </summary>
        protected override RepositoryCacheOptions RepositoryCacheOptions
        {
            get { return _cacheOptions; }
        }

        #region Overrides of RepositoryBase<int,Language>

        protected override ILanguage PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all domains
            return GetAll().FirstOrDefault(x => x.Id == id);
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
            sql.OrderBy<LanguageDto>(dto => dto.Id, SqlSyntax);

            
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

        #region Overrides of PetaPocoRepositoryBase<int,Language>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<LanguageDto>(SqlSyntax);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoLanguage.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {

            var list = new List<string>
                           {
                               //NOTE: There is no constraint between the Language and cmsDictionary/cmsLanguageText tables (?)
                               // but we still need to remove them
                               "DELETE FROM cmsLanguageText WHERE languageId = @Id",
                               "DELETE FROM umbracoLanguage WHERE id = @Id"
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
            ((Entity)entity).AddingEntity();

            var factory = new LanguageFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(ILanguage entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new LanguageFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            entity.ResetDirtyProperties();

            //Clear the cache entries that exist by key/iso
            RuntimeCache.ClearCacheItem(GetCacheIdKey<ILanguage>(entity.IsoCode));
            RuntimeCache.ClearCacheItem(GetCacheIdKey<ILanguage>(entity.CultureName));
        }

        protected override void PersistDeletedItem(ILanguage entity)
        {
            base.PersistDeletedItem(entity);

            //Clear the cache entries that exist by key/iso
            RuntimeCache.ClearCacheItem(GetCacheIdKey<ILanguage>(entity.IsoCode));
            RuntimeCache.ClearCacheItem(GetCacheIdKey<ILanguage>(entity.CultureName));
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
            //use the underlying GetAll which will force cache all domains
            return GetAll().FirstOrDefault(x => x.CultureName.InvariantEquals(cultureName));
        }

        public ILanguage GetByIsoCode(string isoCode)
        {
            //use the underlying GetAll which will force cache all domains
            return GetAll().FirstOrDefault(x => x.IsoCode.InvariantEquals(isoCode));
        }

   
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="DictionaryItem"/>
    /// </summary>
    internal class DictionaryRepository : PetaPocoRepositoryBase<int, IDictionaryItem>, IDictionaryRepository
    {
        private readonly ILanguageRepository _languageRepository;

        public DictionaryRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider syntax, ILanguageRepository languageRepository)
            : base(work, cache, logger, syntax)
		{
		    _languageRepository = languageRepository;
		}

        #region Overrides of RepositoryBase<int,DictionaryItem>

        protected override IDictionaryItem PerformGet(int id)
        {
            var sql = GetBaseQuery(false)
                .Where(GetBaseWhereClause(), new {Id = id})
                .OrderBy<DictionaryDto>(x => x.UniqueId);

            var dto = Database.Fetch<DictionaryDto, LanguageTextDto, DictionaryDto>(new DictionaryLanguageTextRelator().Map, sql).FirstOrDefault();
            if (dto == null)
                return null;

            var entity = ConvertFromDto(dto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)entity).ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<IDictionaryItem> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false).Where("cmsDictionary.pk > 0");
            if (ids.Any())
            {
                sql.Where("cmsDictionary.pk in (@ids)", new { ids = ids });                
            }

            return Database.Fetch<DictionaryDto, LanguageTextDto, DictionaryDto>(new DictionaryLanguageTextRelator().Map, sql)
                    .Select(ConvertFromDto);
        }

        protected override IEnumerable<IDictionaryItem> PerformGetByQuery(IQuery<IDictionaryItem> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IDictionaryItem>(sqlClause, query);
            var sql = translator.Translate();
            sql.OrderBy<DictionaryDto>(x => x.UniqueId);

            return Database.Fetch<DictionaryDto, LanguageTextDto, DictionaryDto>(new DictionaryLanguageTextRelator().Map, sql)
                .Select(ConvertFromDto);
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,DictionaryItem>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if(isCount)
            {
                sql.Select("COUNT(*)")
                    .From<DictionaryDto>();
            }
            else
            {
                sql.Select("*")
                   .From<DictionaryDto>()
                   .LeftJoin<LanguageTextDto>()
                   .On<DictionaryDto, LanguageTextDto>(left => left.UniqueId, right => right.UniqueId);
            }
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "cmsDictionary.pk = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            return new List<string>();
        }

        /// <summary>
        /// Returns the Top Level Parent Guid Id
        /// </summary>
        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.Conventions.Localization.DictionaryItemRootId); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IDictionaryItem entity)
        {
            ((DictionaryItem)entity).AddingEntity();

            foreach (var translation in entity.Translations)
                translation.Value = translation.Value.ToValidXmlString();

            var factory = new DictionaryItemFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            var translationFactory = new DictionaryTranslationFactory(entity.Key, null);
            foreach (var translation in entity.Translations)
            {
                var textDto = translationFactory.BuildDto(translation);
                translation.Id = Convert.ToInt32(Database.Insert(textDto));
                translation.Key = entity.Key;
            }

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IDictionaryItem entity)
        {
            ((Entity)entity).UpdatingEntity();

            foreach (var translation in entity.Translations)
                translation.Value = translation.Value.ToValidXmlString();

            var factory = new DictionaryItemFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            var translationFactory = new DictionaryTranslationFactory(entity.Key, null);
            foreach (var translation in entity.Translations)
            {
                var textDto = translationFactory.BuildDto(translation);
                if(translation.HasIdentity)
                {
                    Database.Update(textDto);
                }
                else
                {
                    translation.Id = Convert.ToInt32(Database.Insert(textDto));
                    translation.Key = entity.Key;
                }
            }

            entity.ResetDirtyProperties();

            //Clear the cache entries that exist by uniqueid/item key
            RepositoryCache.RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(entity.ItemKey));
            RepositoryCache.RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(entity.Key));
        }

        protected override void PersistDeletedItem(IDictionaryItem entity)
        {
            RecursiveDelete(entity.Key);

            Database.Delete<LanguageTextDto>("WHERE UniqueId = @Id", new { Id = entity.Key});
            Database.Delete<DictionaryDto>("WHERE id = @Id", new { Id = entity.Key });

            //Clear the cache entries that exist by uniqueid/item key
            RepositoryCache.RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(entity.ItemKey));
            RepositoryCache.RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(entity.Key));
        }

        private void RecursiveDelete(Guid parentId)
        {
            var list = Database.Fetch<DictionaryDto>("WHERE parent = @ParentId", new {ParentId = parentId});
            foreach (var dto in list)
            {
                RecursiveDelete(dto.UniqueId);

                Database.Delete<LanguageTextDto>("WHERE UniqueId = @Id", new { Id = dto.UniqueId });
                Database.Delete<DictionaryDto>("WHERE id = @Id", new { Id = dto.UniqueId });

                //Clear the cache entries that exist by uniqueid/item key
                RepositoryCache.RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(dto.Key));
                RepositoryCache.RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(dto.UniqueId));
            }
        }

        #endregion

        protected IDictionaryItem ConvertFromDto(DictionaryDto dto)
        {
            var factory = new DictionaryItemFactory();
            var entity = factory.BuildEntity(dto);

            var list = new List<IDictionaryTranslation>();
            foreach (var textDto in dto.LanguageTextDtos)
            {
                var language = _languageRepository.Get(textDto.LanguageId);
                if (language == null)
                    continue;
                var translationFactory = new DictionaryTranslationFactory(dto.UniqueId, language);
                list.Add(translationFactory.BuildEntity(textDto));
            }
            entity.Translations = list;

            return entity;
        }

        public IDictionaryItem Get(Guid uniqueId)
        {
            using (var uniqueIdRepo = new DictionaryByUniqueIdRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                return uniqueIdRepo.Get(uniqueId);    
            }
        }

        public IDictionaryItem Get(string key)
        {
            using (var keyRepo = new DictionaryByKeyRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                return keyRepo.Get(key);    
            }
        }

        private class DictionaryByUniqueIdRepository : SimpleGetRepository<Guid, IDictionaryItem, DictionaryDto>
        {
            private readonly DictionaryRepository _dictionaryRepository;

            public DictionaryByUniqueIdRepository(DictionaryRepository dictionaryRepository, IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
                : base(work, cache, logger, sqlSyntax)
            {
                _dictionaryRepository = dictionaryRepository;
            }

            protected override IEnumerable<DictionaryDto> PerformFetch(Sql sql)
            {
                return Database.Fetch<DictionaryDto, LanguageTextDto, DictionaryDto>(new DictionaryLanguageTextRelator().Map, sql);
            }

            protected override Sql GetBaseQuery(bool isCount)
            {
                return _dictionaryRepository.GetBaseQuery(isCount);
            }

            protected override string GetBaseWhereClause()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("id") + " = @Id";
            }

            protected override IDictionaryItem ConvertToEntity(DictionaryDto dto)
            {
                return _dictionaryRepository.ConvertFromDto(dto);
            }

            protected override object GetBaseWhereClauseArguments(Guid id)
            {
                return new {Id = id};
            }

            protected override string GetWhereInClauseForGetAll()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("id") + " in (@ids)";
            }
        }

        private class DictionaryByKeyRepository : SimpleGetRepository<string, IDictionaryItem, DictionaryDto>
        {
            private readonly DictionaryRepository _dictionaryRepository;

            public DictionaryByKeyRepository(DictionaryRepository dictionaryRepository, IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
                : base(work, cache, logger, sqlSyntax)
            {
                _dictionaryRepository = dictionaryRepository;
            }

            protected override IEnumerable<DictionaryDto> PerformFetch(Sql sql)
            {
                return Database.Fetch<DictionaryDto, LanguageTextDto, DictionaryDto>(new DictionaryLanguageTextRelator().Map, sql);
            }

            protected override Sql GetBaseQuery(bool isCount)
            {
                return _dictionaryRepository.GetBaseQuery(isCount);
            }

            protected override string GetBaseWhereClause()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("key") + " = @Id";
            }

            protected override IDictionaryItem ConvertToEntity(DictionaryDto dto)
            {
                return _dictionaryRepository.ConvertFromDto(dto);
            }

            protected override object GetBaseWhereClauseArguments(string id)
            {
                return new { Id = id };
            }

            protected override string GetWhereInClauseForGetAll()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("key") + " in (@ids)";
            }
        }

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            _languageRepository.Dispose();
        }
    }
}
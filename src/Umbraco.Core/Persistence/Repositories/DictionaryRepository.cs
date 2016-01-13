using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
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
        public DictionaryRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider syntax)
            : base(work, cache, logger, syntax)
        {
        }

        private IRepositoryCachePolicyFactory<IDictionaryItem, int> _cachePolicyFactory;
        protected override IRepositoryCachePolicyFactory<IDictionaryItem, int> CachePolicyFactory
        {
            get
            {
                //custom cache policy which will not cache any results for GetAll
                return _cachePolicyFactory ?? (_cachePolicyFactory = new OnlySingleItemsRepositoryCachePolicyFactory<IDictionaryItem, int>(
                    RuntimeCache,
                    new RepositoryCachePolicyOptions
                    {
                            //allow zero to be cached
                            GetAllCacheAllowZeroCount = true
                    }));
            }
        }

        #region Overrides of RepositoryBase<int,DictionaryItem>

        protected override IDictionaryItem PerformGet(int id)
        {
            var sql = GetBaseQuery(false)
                .Where(GetBaseWhereClause(), new { Id = id })
                .OrderBy<DictionaryDto>(x => x.UniqueId, SqlSyntax);

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
                    .Select(dto => ConvertFromDto(dto));
        }

        protected override IEnumerable<IDictionaryItem> PerformGetByQuery(IQuery<IDictionaryItem> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IDictionaryItem>(sqlClause, query);
            var sql = translator.Translate();
            sql.OrderBy<DictionaryDto>(x => x.UniqueId, SqlSyntax);
            
            return Database.Fetch<DictionaryDto, LanguageTextDto, DictionaryDto>(new DictionaryLanguageTextRelator().Map, sql)
                .Select(x => ConvertFromDto(x));
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,DictionaryItem>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if (isCount)
            {
                sql.Select("COUNT(*)")
                    .From<DictionaryDto>(SqlSyntax);
            }
            else
            {
                sql.Select("*")
                   .From<DictionaryDto>(SqlSyntax)
                   .LeftJoin<LanguageTextDto>(SqlSyntax)
                   .On<DictionaryDto, LanguageTextDto>(SqlSyntax, left => left.UniqueId, right => right.UniqueId);
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

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IDictionaryItem entity)
        {
            var dictionaryItem = ((DictionaryItem) entity);

            dictionaryItem.AddingEntity();

            foreach (var translation in dictionaryItem.Translations)
                translation.Value = translation.Value.ToValidXmlString();

            var factory = new DictionaryItemFactory();
            var dto = factory.BuildDto(dictionaryItem);

            var id = Convert.ToInt32(Database.Insert(dto));
            dictionaryItem.Id = id;

            var translationFactory = new DictionaryTranslationFactory(dictionaryItem.Key);
            foreach (var translation in dictionaryItem.Translations)
            {
                var textDto = translationFactory.BuildDto(translation);
                translation.Id = Convert.ToInt32(Database.Insert(textDto));
                translation.Key = dictionaryItem.Key;
            }

            dictionaryItem.ResetDirtyProperties();            
        }

        protected override void PersistUpdatedItem(IDictionaryItem entity)
        {
            ((Entity)entity).UpdatingEntity();

            foreach (var translation in entity.Translations)
                translation.Value = translation.Value.ToValidXmlString();

            var factory = new DictionaryItemFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            var translationFactory = new DictionaryTranslationFactory(entity.Key);
            foreach (var translation in entity.Translations)
            {
                var textDto = translationFactory.BuildDto(translation);
                if (translation.HasIdentity)
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
            RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(entity.ItemKey));
            RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(entity.Key));
        }

        protected override void PersistDeletedItem(IDictionaryItem entity)
        {
            RecursiveDelete(entity.Key);

            Database.Delete<LanguageTextDto>("WHERE UniqueId = @Id", new { Id = entity.Key });
            Database.Delete<DictionaryDto>("WHERE id = @Id", new { Id = entity.Key });

            //Clear the cache entries that exist by uniqueid/item key
            RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(entity.ItemKey));
            RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(entity.Key));
        }

        private void RecursiveDelete(Guid parentId)
        {
            var list = Database.Fetch<DictionaryDto>("WHERE parent = @ParentId", new { ParentId = parentId });
            foreach (var dto in list)
            {
                RecursiveDelete(dto.UniqueId);

                Database.Delete<LanguageTextDto>("WHERE UniqueId = @Id", new { Id = dto.UniqueId });
                Database.Delete<DictionaryDto>("WHERE id = @Id", new { Id = dto.UniqueId });

                //Clear the cache entries that exist by uniqueid/item key
                RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(dto.Key));
                RuntimeCache.ClearCacheItem(GetCacheIdKey<IDictionaryItem>(dto.UniqueId));
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
                if (textDto.LanguageId <= 0)
                    continue;

                var translationFactory = new DictionaryTranslationFactory(dto.UniqueId);
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
        
        private IEnumerable<IDictionaryItem> GetRootDictionaryItems()
        {
            var query = Query<IDictionaryItem>.Builder.Where(x => x.ParentId == null);
            return GetByQuery(query);
        }

        public IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId)
        {
            //This methods will look up children at each level, since we do not store a path for dictionary (ATM), we need to do a recursive
            // lookup to get descendants. Currently this is the most efficient way to do it

            Func<Guid[], IEnumerable<IEnumerable<IDictionaryItem>>> getItemsFromParents = guids =>
            {
                //needs to be in groups of 2000 because we are doing an IN clause and there's a max parameter count that can be used.
                return guids.InGroupsOf(2000)
                    .Select(@group =>
                    {
                        var sqlClause = GetBaseQuery(false)
                            .Where<DictionaryDto>(x => x.Parent != null)
                            .Where(string.Format("{0} IN (@parentIds)", SqlSyntax.GetQuotedColumnName("parent")), new { parentIds = @group });

                        var translator = new SqlTranslator<IDictionaryItem>(sqlClause, Query<IDictionaryItem>.Builder);
                        var sql = translator.Translate();
                        sql.OrderBy<DictionaryDto>(x => x.UniqueId, SqlSyntax);

                        return Database.Fetch<DictionaryDto, LanguageTextDto, DictionaryDto>(new DictionaryLanguageTextRelator().Map, sql)
                            .Select(x => ConvertFromDto(x));
                    });
            };

            var childItems = parentId.HasValue == false
                ? new[] { GetRootDictionaryItems() }
                : getItemsFromParents(new[] { parentId.Value });

            return childItems.SelectRecursive(items => getItemsFromParents(items.Select(x => x.Key).ToArray())).SelectMany(items => items);

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
                return new { Id = id };
            }

            protected override string GetWhereInClauseForGetAll()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("id") + " in (@ids)";
            }

            private IRepositoryCachePolicyFactory<IDictionaryItem, Guid> _cachePolicyFactory;
            protected override IRepositoryCachePolicyFactory<IDictionaryItem, Guid> CachePolicyFactory
            {
                get
                {
                    //custom cache policy which will not cache any results for GetAll
                    return _cachePolicyFactory ?? (_cachePolicyFactory = new OnlySingleItemsRepositoryCachePolicyFactory<IDictionaryItem, Guid>(
                        RuntimeCache,
                        new RepositoryCachePolicyOptions
                        {
                            //allow zero to be cached
                            GetAllCacheAllowZeroCount = true
                        }));
                }
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

            private IRepositoryCachePolicyFactory<IDictionaryItem, string> _cachePolicyFactory;
            protected override IRepositoryCachePolicyFactory<IDictionaryItem, string> CachePolicyFactory
            {
                get
                {
                    //custom cache policy which will not cache any results for GetAll
                    return _cachePolicyFactory ?? (_cachePolicyFactory = new OnlySingleItemsRepositoryCachePolicyFactory<IDictionaryItem, string>(
                        RuntimeCache,
                        new RepositoryCachePolicyOptions
                        {
                            //allow zero to be cached
                            GetAllCacheAllowZeroCount = true
                        }));
                }
            }
        }

       
    }
}
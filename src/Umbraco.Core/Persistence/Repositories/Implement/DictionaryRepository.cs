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
    /// Represents a repository for doing CRUD operations for <see cref="DictionaryItem"/>
    /// </summary>
    internal class DictionaryRepository : NPocoRepositoryBase<int, IDictionaryItem>, IDictionaryRepository
    {
        public DictionaryRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override IRepositoryCachePolicy<IDictionaryItem, int> CreateCachePolicy()
        {
            var options = new RepositoryCachePolicyOptions
            {
                //allow zero to be cached
                GetAllCacheAllowZeroCount = true
            };

            return new SingleItemsOnlyRepositoryCachePolicy<IDictionaryItem, int>(GlobalIsolatedCache, ScopeAccessor, options);
        }

        #region Overrides of RepositoryBase<int,DictionaryItem>

        protected override IDictionaryItem PerformGet(int id)
        {
            var sql = GetBaseQuery(false)
                .Where(GetBaseWhereClause(), new { id = id })
                .OrderBy<DictionaryDto>(x => x.UniqueId);

            var dto = Database
                .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
                .FirstOrDefault();

            if (dto == null)
                return null;

            var entity = ConvertFromDto(dto);

            // reset dirty initial properties (U4-1946)
            ((EntityBase)entity).ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<IDictionaryItem> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false).Where<DictionaryDto>(x => x.PrimaryKey > 0);
            if (ids.Any())
            {
                sql.WhereIn<DictionaryDto>(x => x.PrimaryKey, ids);
            }

            return Database
                .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
                .Select(ConvertFromDto);
        }

        protected override IEnumerable<IDictionaryItem> PerformGetByQuery(IQuery<IDictionaryItem> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IDictionaryItem>(sqlClause, query);
            var sql = translator.Translate();
            sql.OrderBy<DictionaryDto>(x => x.UniqueId);

            return Database
                .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql)
                .Select(ConvertFromDto);
        }

        #endregion

        #region Overrides of NPocoRepositoryBase<int,DictionaryItem>

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();
            if (isCount)
            {
                sql.SelectCount()
                    .From<DictionaryDto>();
            }
            else
            {
                sql.SelectAll()
                   .From<DictionaryDto>()
                   .LeftJoin<LanguageTextDto>()
                   .On<DictionaryDto, LanguageTextDto>(left => left.UniqueId, right => right.UniqueId);
            }
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return $"{Constants.DatabaseSchema.Tables.DictionaryEntry}.pk = @id";
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

            var dto = DictionaryItemFactory.BuildDto(dictionaryItem);

            var id = Convert.ToInt32(Database.Insert(dto));
            dictionaryItem.Id = id;

            foreach (var translation in dictionaryItem.Translations)
            {
                var textDto = DictionaryTranslationFactory.BuildDto(translation, dictionaryItem.Key);
                translation.Id = Convert.ToInt32(Database.Insert(textDto));
                translation.Key = dictionaryItem.Key;
            }

            dictionaryItem.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IDictionaryItem entity)
        {
            entity.UpdatingEntity();

            foreach (var translation in entity.Translations)
                translation.Value = translation.Value.ToValidXmlString();

            var dto = DictionaryItemFactory.BuildDto(entity);

            Database.Update(dto);

            foreach (var translation in entity.Translations)
            {
                var textDto = DictionaryTranslationFactory.BuildDto(translation, entity.Key);
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
            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(entity.ItemKey));
            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, Guid>(entity.Key));
        }

        protected override void PersistDeletedItem(IDictionaryItem entity)
        {
            RecursiveDelete(entity.Key);

            Database.Delete<LanguageTextDto>("WHERE UniqueId = @Id", new { Id = entity.Key });
            Database.Delete<DictionaryDto>("WHERE id = @Id", new { Id = entity.Key });

            //Clear the cache entries that exist by uniqueid/item key
            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(entity.ItemKey));
            IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, Guid>(entity.Key));

            entity.DeleteDate = DateTime.Now;
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
                IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, string>(dto.Key));
                IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IDictionaryItem, Guid>(dto.UniqueId));
            }
        }

        #endregion

        protected IDictionaryItem ConvertFromDto(DictionaryDto dto)
        {
            var entity = DictionaryItemFactory.BuildEntity(dto);

            entity.Translations = dto.LanguageTextDtos.EmptyNull()
                .Where(x => x.LanguageId > 0)
                .Select(x => DictionaryTranslationFactory.BuildEntity(x, dto.UniqueId))
                .ToList();

            return entity;
        }

        public IDictionaryItem Get(Guid uniqueId)
        {
            var uniqueIdRepo = new DictionaryByUniqueIdRepository(this, ScopeAccessor, AppCaches, Logger);
            return uniqueIdRepo.Get(uniqueId);
        }

        public IDictionaryItem Get(string key)
        {
            var keyRepo = new DictionaryByKeyRepository(this, ScopeAccessor, AppCaches, Logger);
            return keyRepo.Get(key);
        }

        private IEnumerable<IDictionaryItem> GetRootDictionaryItems()
        {
            var query = Query<IDictionaryItem>().Where(x => x.ParentId == null);
            return Get(query);
        }

        public Dictionary<string, Guid> GetDictionaryItemKeyMap()
        {
            var columns = new[] { "key", "id" }.Select(x => (object) SqlSyntax.GetQuotedColumnName(x)).ToArray();
            var sql = Sql().Select(columns).From<DictionaryDto>();
            return Database.Fetch<DictionaryItemKeyIdDto>(sql).ToDictionary(x => x.Key, x => x.Id);
        }

        private class DictionaryItemKeyIdDto
        {
            public string Key { get; set; }
            public Guid Id { get; set; }
        }

        public IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId)
        {
            //This methods will look up children at each level, since we do not store a path for dictionary (ATM), we need to do a recursive
            // lookup to get descendants. Currently this is the most efficient way to do it

            Func<Guid[], IEnumerable<IEnumerable<IDictionaryItem>>> getItemsFromParents = guids =>
            {
                return guids.InGroupsOf(Constants.Sql.MaxParameterCount)
                    .Select(group =>
                    {
                        var sqlClause = GetBaseQuery(false)
                            .Where<DictionaryDto>(x => x.Parent != null)
                            .WhereIn<DictionaryDto>(x => x.Parent, group);

                        var translator = new SqlTranslator<IDictionaryItem>(sqlClause, Query<IDictionaryItem>());
                        var sql = translator.Translate();
                        sql.OrderBy<DictionaryDto>(x => x.UniqueId);

                        return Database
                            .FetchOneToMany<DictionaryDto>(x=> x.LanguageTextDtos, sql)
                            .Select(ConvertFromDto);
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

            public DictionaryByUniqueIdRepository(DictionaryRepository dictionaryRepository, IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
                : base(scopeAccessor, cache, logger)
            {
                _dictionaryRepository = dictionaryRepository;
            }

            protected override IEnumerable<DictionaryDto> PerformFetch(Sql sql)
            {
                return Database
                    .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql);
            }

            protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
            {
                return _dictionaryRepository.GetBaseQuery(isCount);
            }

            protected override string GetBaseWhereClause()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("id") + " = @id";
            }

            protected override IDictionaryItem ConvertToEntity(DictionaryDto dto)
            {
                return _dictionaryRepository.ConvertFromDto(dto);
            }

            protected override object GetBaseWhereClauseArguments(Guid id)
            {
                return new { id = id };
            }

            protected override string GetWhereInClauseForGetAll()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("id") + " in (@ids)";
            }

            protected override IRepositoryCachePolicy<IDictionaryItem, Guid> CreateCachePolicy()
            {
                var options = new RepositoryCachePolicyOptions
                {
                    //allow zero to be cached
                    GetAllCacheAllowZeroCount = true
                };

                return new SingleItemsOnlyRepositoryCachePolicy<IDictionaryItem, Guid>(GlobalIsolatedCache, ScopeAccessor, options);
            }
        }

        private class DictionaryByKeyRepository : SimpleGetRepository<string, IDictionaryItem, DictionaryDto>
        {
            private readonly DictionaryRepository _dictionaryRepository;

            public DictionaryByKeyRepository(DictionaryRepository dictionaryRepository, IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
                : base(scopeAccessor, cache, logger)
            {
                _dictionaryRepository = dictionaryRepository;
            }

            protected override IEnumerable<DictionaryDto> PerformFetch(Sql sql)
            {
                return Database
                    .FetchOneToMany<DictionaryDto>(x => x.LanguageTextDtos, sql);
            }

            protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
            {
                return _dictionaryRepository.GetBaseQuery(isCount);
            }

            protected override string GetBaseWhereClause()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("key") + " = @id";
            }

            protected override IDictionaryItem ConvertToEntity(DictionaryDto dto)
            {
                return _dictionaryRepository.ConvertFromDto(dto);
            }

            protected override object GetBaseWhereClauseArguments(string id)
            {
                return new { id = id };
            }

            protected override string GetWhereInClauseForGetAll()
            {
                return "cmsDictionary." + SqlSyntax.GetQuotedColumnName("key") + " in (@ids)";
            }

            protected override IRepositoryCachePolicy<IDictionaryItem, string> CreateCachePolicy()
            {
                var options = new RepositoryCachePolicyOptions
                {
                    //allow zero to be cached
                    GetAllCacheAllowZeroCount = true
                };

                return new SingleItemsOnlyRepositoryCachePolicy<IDictionaryItem, string>(GlobalIsolatedCache, ScopeAccessor, options);
            }
        }
    }
}

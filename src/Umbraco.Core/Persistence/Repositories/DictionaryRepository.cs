using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class DictionaryRepository : PetaPocoRepositoryBase<int, DictionaryItem>, IDictionaryRepository
    {
        public DictionaryRepository(IUnitOfWork work) : base(work)
        {
        }

        public DictionaryRepository(IUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,DictionaryItem>

        protected override DictionaryItem PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<DictionaryItem> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<DictionaryItem> PerformGetByQuery(IQuery<DictionaryItem> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,DictionaryItem>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*");
            sql.From("cmsDictionary");
            sql.InnerJoin("cmsLanguageText ON ([cmsDictionary].[id] = [cmsLanguageText].[UniqueId])");
            return sql;
        }

        protected override Sql GetBaseWhereClause(object id)
        {
            var sql = new Sql();
            sql.Where("[cmsDictionary].[id] = @Id", new { Id = id });
            return sql;
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
            get { return new Guid("41c7638d-f529-4bff-853e-59a0c2fb1bde"); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(DictionaryItem entity)
        {
            throw new NotImplementedException();
        }

        protected override void PersistUpdatedItem(DictionaryItem entity)
        {
            throw new NotImplementedException();
        }

        protected override void PersistDeletedItem(DictionaryItem entity)
        {
            RecursiveDelete(entity.Key);

            Database.Delete<LanguageTextDto>("WHERE UniqueId = @Id", new { Id = entity.Key});

            Database.Delete<DictionaryDto>("WHERE id = @Id", new { Id = entity.Key });
        }

        private void RecursiveDelete(Guid parentId)
        {
            var list = Database.Fetch<DictionaryDto>("WHERE parent = @ParentId", new {ParentId = parentId});
            foreach (var dto in list)
            {
                RecursiveDelete(dto.Id);

                Database.Delete<LanguageTextDto>("WHERE UniqueId = @Id", new { Id = dto.Id });
                Database.Delete<DictionaryDto>("WHERE id = @Id", new { Id = dto.Id });
            }
        }

        #endregion
    }
}
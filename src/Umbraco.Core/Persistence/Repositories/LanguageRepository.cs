using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class LanguageRepository : PetaPocoRepositoryBase<int, Language>, ILanguageRepository
    {
        public LanguageRepository(IUnitOfWork work) : base(work)
        {
        }

        public LanguageRepository(IUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,Language>

        protected override Language PerformGet(int id)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<Language> PerformGetAll(params int[] ids)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<Language> PerformGetByQuery(IQuery<Language> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,Language>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*");
            sql.From("cmsLanguageText");
            return sql;
        }

        protected override Sql GetBaseWhereClause(object id)
        {
            var sql = new Sql();
            sql.Where("[cmsLanguageText].[id] = @Id", new { Id = id });
            return sql;
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            //NOTE: There is no constraint between the Language and cmsDictionary/cmsLanguageText tables (?)
            var list = new List<string>
                           {
                               string.Format("DELETE FROM umbracoLanguage WHERE id = @Id")
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(Language entity)
        {
            throw new NotImplementedException();
        }

        protected override void PersistUpdatedItem(Language entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
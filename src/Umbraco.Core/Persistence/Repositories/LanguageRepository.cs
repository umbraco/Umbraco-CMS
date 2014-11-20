using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="Language"/>
    /// </summary>
    internal class LanguageRepository : PetaPocoRepositoryBase<int, ILanguage>, ILanguageRepository
    {
		public LanguageRepository(IDatabaseUnitOfWork work)
			: base(work)
        {
        }

		public LanguageRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
            : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,Language>

        protected override ILanguage PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var languageDto = Database.FirstOrDefault<LanguageDto>(sql);
            if (languageDto == null)
                return null;

            var factory = new LanguageFactory();
            var entity = factory.BuildEntity(languageDto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)entity).ResetDirtyProperties(false);

            return entity;
        }

        //TODO: Fix this up so there is no N+1

        protected override IEnumerable<ILanguage> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var dtos = Database.Fetch<LanguageDto>("WHERE id > 0");
                foreach (var dto in dtos)
                {
                    yield return Get(dto.Id);
                }
            }
        }

        //TODO: Fix this up so there is no N+1

        protected override IEnumerable<ILanguage> PerformGetByQuery(IQuery<ILanguage> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<ILanguage>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<LanguageDto>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,Language>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<LanguageDto>();
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
        }

        #endregion
    }
}
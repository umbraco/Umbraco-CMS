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
    internal class UserSectionRepository : PetaPocoRepositoryBase<Tuple<int, string>, UserSection>, IUserSectionRepository, IUnitOfWorkRepository
    {
        public UserSectionRepository(IDatabaseUnitOfWork work) : base(work)
        {
        }

        public UserSectionRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        protected override UserSection PerformGet(Tuple<int, string> id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            var dto = Database.First<User2AppDto>(sql);
            if (dto == null)
                return null;
            var factory = new UserSectionFactory();
            var entity = factory.BuildEntity(dto);
            return entity;
        }

        protected override IEnumerable<UserSection> PerformGetAll(params Tuple<int, string>[] ids)
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
                var factory = new UserSectionFactory();
                foreach (var u in Database.Fetch<User2AppDto>("WHERE user > 0")
                    .Select(factory.BuildEntity))
                {
                    yield return u;
                }
            }
        }

        protected override IEnumerable<UserSection> PerformGetByQuery(IQuery<UserSection> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<UserSection>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<User2AppDto>(sql);

            return dtos.Select(dto => Get(
                new Tuple<int, string>(dto.UserId, dto.AppAlias)));
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<User2AppDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "user = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoUser2app WHERE user = @Id AND app = @Section"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(UserSection entity)
        {
            var factory = new UserSectionFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;
        }

        protected override void PersistUpdatedItem(UserSection entity)
        {
            var factory = new UserSectionFactory();
            var dto = factory.BuildDto(entity);
            Database.Update(dto);
        }

        protected override void PersistDeletedItem(UserSection entity)
        {
            var deletes = GetDeleteClauses();
            foreach (var delete in deletes)
            {
                //ensure that we include SectionAlias since its a composite key
                Database.Execute(delete, new { Id = entity.Id, Section = entity.SectionAlias });
            }
        }
    }
}
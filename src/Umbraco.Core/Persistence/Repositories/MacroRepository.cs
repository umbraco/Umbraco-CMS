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
    internal class MacroRepository : PetaPocoRepositoryBase<int, Macro>
    {
        public MacroRepository(IDatabaseUnitOfWork work) : base(work)
        {
        }

        public MacroRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        protected override Macro PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var macroDto = Database.First<MacroDto>(sql);
            if (macroDto == null)
                return null;

            var factory = new MacroFactory();
            var entity = factory.BuildEntity(macroDto);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<Macro> PerformGetAll(params int[] ids)
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
                var macroDtos = Database.Fetch<MacroDto>("WHERE id > 0");
                foreach (var macroDto in macroDtos)
                {
                    yield return Get(macroDto.Id);
                }
            }
        }

        protected override IEnumerable<Macro> PerformGetByQuery(IQuery<Macro> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<Macro>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<Macro>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<MacroDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM cmsMacro WHERE id = @Id"                               
                };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(Macro entity)
        {
            ((Entity)entity).AddingEntity();

            var factory = new MacroFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(Macro entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new MacroFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }
    }
}
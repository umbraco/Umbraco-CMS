using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class MacroRepository : PetaPocoRepositoryBase<int, IMacro>, IMacroRepository
    {
        public MacroRepository(IDatabaseUnitOfWork work) : base(work)
        {
        }

        public MacroRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        protected override IMacro PerformGet(int id)
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
            ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);

            return entity;
        }

        protected override IEnumerable<IMacro> PerformGetAll(params int[] ids)
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

        protected override IEnumerable<IMacro> PerformGetByQuery(IQuery<IMacro> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMacro>(sqlClause, query);
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
            if (isCount)
            {
                sql.Select("COUNT(*)").From<MacroDto>();
            }
            else
            {
                return GetBaseQuery("*");
            }
            return sql;
        }

        private static Sql GetBaseQuery(string columns)
        {
            var sql = new Sql();
            sql.Select(columns)
                      .From<MacroDto>()
                      .LeftJoin<MacroPropertyDto>()
                      .On<MacroDto, MacroPropertyDto>(left => left.Id, right => right.Macro);
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

        protected override void PersistNewItem(IMacro entity)
        {
            ((Entity)entity).AddingEntity();

            var factory = new MacroFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            foreach (var propDto in dto.MacroPropertyDtos)
            {
                //need to set the id explicitly here
                propDto.Macro = id;
                Database.Insert(propDto);
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMacro entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new MacroFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            //update the sections if they've changed
            var macro = (Macro)entity;
            if (macro.IsPropertyDirty("Properties"))
            {
                //for any that exist on the object, we need to determine if we need to update or insert
                foreach (var propDto in dto.MacroPropertyDtos)
                {
                    if (macro.AddedProperties.Contains(propDto.Alias))
                    {
                        //we need to insert since this was added  
                        Database.Insert(propDto);
                    }
                    else
                    {
                        Database.Update(propDto);
                    }
                }

                //now we need to delete any props that have been removed
                foreach (var propAlias in macro.RemovedProperties)
                {
                    //delete the property
                    Database.Delete<MacroPropertyDto>("WHERE macro=@macroId AND macroPropertyAlias=@propAlias",
                        new { macroId = macro.Id, propAlias = propAlias });
                }
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }
    }
}
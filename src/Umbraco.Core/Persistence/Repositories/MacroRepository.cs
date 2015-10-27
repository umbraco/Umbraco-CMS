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
    internal class MacroRepository : PetaPocoRepositoryBase<int, IMacro>, IMacroRepository
    {

        public MacroRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        protected override IMacro PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var macroDto = Database.Fetch<MacroDto, MacroPropertyDto, MacroDto>(new MacroPropertyRelator().Map, sql).FirstOrDefault();
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
                return PerformGetAllOnIds(ids);
            }

            var sql = GetBaseQuery(false);

            return ConvertFromDtos(Database.Fetch<MacroDto, MacroPropertyDto, MacroDto>(new MacroPropertyRelator().Map, sql))
                .ToArray();// we don't want to re-iterate again!
        }

        private IEnumerable<IMacro> PerformGetAllOnIds(params int[] ids)
        {
            if (ids.Any() == false) yield break;
            foreach (var id in ids)
            {
                yield return Get(id);
            }
        }

        private IEnumerable<IMacro> ConvertFromDtos(IEnumerable<MacroDto> dtos)
        {
            var factory = new MacroFactory();
            foreach (var entity in dtos.Select(factory.BuildEntity))
            {
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                ((TracksChangesEntityBase)entity).ResetDirtyProperties(false);

                yield return entity;
            }
        }

        protected override IEnumerable<IMacro> PerformGetByQuery(IQuery<IMacro> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMacro>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<MacroDto, MacroPropertyDto, MacroDto>(new MacroPropertyRelator().Map, sql);

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
                return GetBaseQuery();
            }
            return sql;
        }

        private static Sql GetBaseQuery()
        {
            var sql = new Sql();
            sql.Select("*")
               .From<MacroDto>()
               .LeftJoin<MacroPropertyDto>()
               .On<MacroDto, MacroPropertyDto>(left => left.Id, right => right.Macro);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "cmsMacro.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM cmsMacroProperty WHERE macro = @Id",
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
                var propId = Convert.ToInt32(Database.Insert(propDto));
                entity.Properties[propDto.Alias].Id = propId;
            }

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMacro entity)
        {
            ((Entity)entity).UpdatingEntity();

            var factory = new MacroFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            //update the properties if they've changed
            var macro = (Macro)entity;
            if (macro.IsPropertyDirty("Properties") || macro.Properties.Any(x => x.IsDirty()))
            {
                //now we need to delete any props that have been removed
                foreach (var propAlias in macro.RemovedProperties)
                {
                    //delete the property
                    Database.Delete<MacroPropertyDto>("WHERE macro=@macroId AND macroPropertyAlias=@propAlias",
                        new { macroId = macro.Id, propAlias = propAlias });
                }

                //for any that exist on the object, we need to determine if we need to update or insert
                foreach (var propDto in dto.MacroPropertyDtos)
                {
                    if (macro.AddedProperties.Contains(propDto.Alias))
                    {
                        //we need to insert since this was added  and re-assign the new id
                        var propId = Convert.ToInt32(Database.Insert(propDto));
                        macro.Properties[propDto.Alias].Id = propId;
                    }
                    else
                    {
                        //This will only work if the Alias hasn't been changed
                        if (macro.Properties.ContainsKey(propDto.Alias))
                        {
                            //only update if it's dirty
                            if (macro.Properties[propDto.Alias].IsDirty())
                            {
                                Database.Update(propDto);
                            }
                        }
                        else
                        {
                            var property = macro.Properties.FirstOrDefault(x => x.Id == propDto.Id);
                            if (property != null && property.IsDirty())
                            {
                                Database.Update(propDto);
                            }
                        }
                    }
                }

                
            }

            entity.ResetDirtyProperties();
        }

        //public IEnumerable<IMacro> GetAll(params string[] aliases)
        //{
        //    if (aliases.Any())
        //    {
        //        var q = new Query<IMacro>();
        //        foreach (var alias in aliases)
        //        {
        //            q.Where(macro => macro.Alias == alias);
        //        }

        //        var wheres = string.Join(" OR ", q.WhereClauses());
        //    }
        //    else
        //    {
        //        return GetAll(new int[] {});
        //    }
            
        //}
    }
}
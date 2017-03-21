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

        public MacroRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        protected override IMacro PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            return GetBySql(sql);
        }

        public IMacro Get(Guid id)
        {
            var sql = GetBaseQuery().Where("uniqueId=@Id", new { Id = id });
            return GetBySql(sql);
        }

        private IMacro GetBySql(Sql sql)
        {
            //must be sorted this way for the relator to work
            sql.OrderBy<MacroDto>(x => x.Id, SqlSyntax);

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

        public IEnumerable<IMacro> GetAll(params Guid[] ids)
        {
            return ids.Length > 0 ? ids.Select(Get) : GetAllNoIds();
        }

        public bool Exists(Guid id)
        {
            return Get(id) != null;
        }

        protected override IEnumerable<IMacro> PerformGetAll(params int[] ids)
        {
            return ids.Length > 0 ? ids.Select(Get) : GetAllNoIds();
        }

        private IEnumerable<IMacro> GetAllNoIds()
        {
            var sql = GetBaseQuery(false)
                //must be sorted this way for the relator to work
                .OrderBy<MacroDto>(x => x.Id, SqlSyntax);

            return ConvertFromDtos(Database.Fetch<MacroDto, MacroPropertyDto, MacroDto>(new MacroPropertyRelator().Map, sql))
                .ToArray();// we don't want to re-iterate again!
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

            //must be sorted this way for the relator to work
            sql.OrderBy<MacroDto>(x => x.Id, SqlSyntax);

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

        private Sql GetBaseQuery()
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
                // need to set the id explicitly here
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
            var macro = (Macro) entity;
            if (macro.IsPropertyDirty("Properties") || macro.Properties.Any(x => x.IsDirty()))
            {
                var ids = dto.MacroPropertyDtos.Where(x => x.Id > 0).Select(x => x.Id).ToArray();
                if (ids.Length > 0)
                    Database.Delete<MacroPropertyDto>("WHERE macro=@macro AND id NOT IN (@ids)", new { macro = dto.Id, ids });
                else
                    Database.Delete<MacroPropertyDto>("WHERE macro=@macro", new { macro = dto.Id });

                // detect new aliases, replace with temp aliases
                // this ensures that we don't have collisions, ever
                var aliases = new Dictionary<string, string>();
                foreach (var propDto in dto.MacroPropertyDtos)
                {
                    var prop = macro.Properties.FirstOrDefault(x => x.Id == propDto.Id);
                    if (prop == null) throw new Exception("oops: property.");
                    if (propDto.Id == 0 || prop.IsPropertyDirty("Alias"))
                    {
                        var tempAlias = Guid.NewGuid().ToString("N").Substring(0, 8);
                        aliases[tempAlias] = propDto.Alias;
                        propDto.Alias = tempAlias;
                    }
                }

                // insert or update existing properties, with temp aliases
                foreach (var propDto in dto.MacroPropertyDtos)
                {
                    if (propDto.Id == 0)
                    {
                        // insert
                        propDto.Id = Convert.ToInt32(Database.Insert(propDto));
                        macro.Properties[aliases[propDto.Alias]].Id = propDto.Id;
                    }
                    else
                    {
                        // update
                        var property = macro.Properties.FirstOrDefault(x => x.Id == propDto.Id);
                        if (property == null) throw new Exception("oops: property.");
                        if (property.IsDirty())
                            Database.Update(propDto);
                    }
                }

                // replace the temp aliases with the real ones
                foreach (var propDto in dto.MacroPropertyDtos)
                {
                    if (aliases.ContainsKey(propDto.Alias) == false) continue;

                    propDto.Alias = aliases[propDto.Alias];
                    Database.Update(propDto);
                }
            }

            entity.ResetDirtyProperties();
        }
    }
}
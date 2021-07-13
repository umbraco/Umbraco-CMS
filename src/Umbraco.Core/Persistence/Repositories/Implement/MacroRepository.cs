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
    internal class MacroRepository : NPocoRepositoryBase<int, IMacro>, IMacroRepository
    {
        public MacroRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override IMacro PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { id });
            return GetBySql(sql);
        }

        public IMacro Get(Guid id)
        {
            var sql = GetBaseQuery().Where<MacroDto>(x => x.UniqueId == id);
            return GetBySql(sql);
        }

        private IMacro GetBySql(Sql sql)
        {
            var macroDto = Database
                .FetchOneToMany<MacroDto>(x => x.MacroPropertyDtos, sql)
                .FirstOrDefault();

            if (macroDto == null)
                return null;

            var entity = MacroFactory.BuildEntity(macroDto);

            // reset dirty initial properties (U4-1946)
            ((BeingDirtyBase)entity).ResetDirtyProperties(false);

            return entity;
        }

        public IEnumerable<IMacro> GetMany(params Guid[] ids)
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
                .OrderBy<MacroDto>(x => x.Id);

            return Database
                .FetchOneToMany<MacroDto>(x => x.MacroPropertyDtos, sql)
                .Transform(ConvertFromDtos)
                .ToArray(); // do it now and once
        }

        private IEnumerable<IMacro> ConvertFromDtos(IEnumerable<MacroDto> dtos)
        {

            foreach (var entity in dtos.Select(MacroFactory.BuildEntity))
            {
                // reset dirty initial properties (U4-1946)
                ((BeingDirtyBase)entity).ResetDirtyProperties(false);

                yield return entity;
            }
        }

        protected override IEnumerable<IMacro> PerformGetByQuery(IQuery<IMacro> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMacro>(sqlClause, query);
            var sql = translator.Translate();

            return Database
                .FetchOneToMany<MacroDto>(x => x.MacroPropertyDtos, sql)
                .Select(x => Get(x.Id));
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return isCount ? Sql().SelectCount().From<MacroDto>() : GetBaseQuery();
        }

        private Sql<ISqlContext> GetBaseQuery()
        {
            return Sql()
                .SelectAll()
                .From<MacroDto>()
                .LeftJoin<MacroPropertyDto>()
                .On<MacroDto, MacroPropertyDto>(left => left.Id, right => right.Macro);
        }

        protected override string GetBaseWhereClause()
        {
            return $"{Constants.DatabaseSchema.Tables.Macro}.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM cmsMacroProperty WHERE macro = @id",
                    "DELETE FROM cmsMacro WHERE id = @id"
                };
            return list;
        }

        protected override Guid NodeObjectTypeId => throw new NotImplementedException();

        protected override void PersistNewItem(IMacro entity)
        {
            entity.AddingEntity();

            var dto = MacroFactory.BuildDto(entity);

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
            entity.UpdatingEntity();
            var dto = MacroFactory.BuildDto(entity);

            Database.Update(dto);

            //update the properties if they've changed
            var macro = (Macro)entity;
            if (macro.IsPropertyDirty("Properties") || macro.Properties.Values.Any(x => x.IsDirty()))
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
                    var prop = macro.Properties.Values.FirstOrDefault(x => x.Id == propDto.Id);
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
                        var property = macro.Properties.Values.FirstOrDefault(x => x.Id == propDto.Id);
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

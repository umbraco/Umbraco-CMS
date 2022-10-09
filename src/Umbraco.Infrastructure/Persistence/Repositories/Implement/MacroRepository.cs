using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class MacroRepository : EntityRepositoryBase<int, IMacro>, IMacroWithAliasRepository
{
    private readonly IRepositoryCachePolicy<IMacro, string> _macroByAliasCachePolicy;
    private readonly IShortStringHelper _shortStringHelper;

    public MacroRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<MacroRepository> logger,
        IShortStringHelper shortStringHelper)
        : base(scopeAccessor, cache, logger)
    {
        _shortStringHelper = shortStringHelper;
        _macroByAliasCachePolicy =
            new DefaultRepositoryCachePolicy<IMacro, string>(GlobalIsolatedCache, ScopeAccessor, DefaultOptions);
    }

    public IMacro? Get(Guid id)
    {
        Sql<ISqlContext> sql = GetBaseQuery().Where<MacroDto>(x => x.UniqueId == id);
        return GetBySql(sql);
    }

    public IEnumerable<IMacro> GetMany(params Guid[]? ids) =>
        ids?.Length > 0 ? ids.Select(Get).WhereNotNull() : GetAllNoIds();

    public bool Exists(Guid id) => Get(id) != null;

    public IMacro? GetByAlias(string alias) =>
        _macroByAliasCachePolicy.Get(alias, PerformGetByAlias, PerformGetAllByAlias);

    public IEnumerable<IMacro> GetAllByAlias(string[] aliases)
    {
        if (aliases.Any() is false)
        {
            return base.GetMany();
        }

        return _macroByAliasCachePolicy.GetAll(aliases, PerformGetAllByAlias);
    }

    protected override IMacro? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), new { id });
        return GetBySql(sql);
    }

    protected override IEnumerable<IMacro> PerformGetAll(params int[]? ids) =>
        ids?.Length > 0 ? ids.Select(Get).WhereNotNull() : GetAllNoIds();

    protected override IEnumerable<IMacro> PerformGetByQuery(IQuery<IMacro> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IMacro>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        return Database
            .FetchOneToMany<MacroDto>(x => x.MacroPropertyDtos, sql)
            .Select(x => Get(x.Id)!);
    }

    private IMacro? GetBySql(Sql sql)
    {
        MacroDto? macroDto = Database
            .FetchOneToMany<MacroDto>(x => x.MacroPropertyDtos, sql)
            .FirstOrDefault();

        if (macroDto == null)
        {
            return null;
        }

        IMacro entity = MacroFactory.BuildEntity(_shortStringHelper, macroDto);

        // reset dirty initial properties (U4-1946)
        ((BeingDirtyBase)entity).ResetDirtyProperties(false);

        return entity;
    }

    private IMacro? PerformGetByAlias(string? alias)
    {
        IQuery<IMacro> query = Query<IMacro>().Where(x => x.Alias.Equals(alias));
        return PerformGetByQuery(query).FirstOrDefault();
    }

    private IEnumerable<IMacro> PerformGetAllByAlias(params string[]? aliases)
    {
        if (aliases is null || aliases.Any() is false)
        {
            return base.GetMany();
        }

        IQuery<IMacro> query = Query<IMacro>().Where(x => aliases.Contains(x.Alias));
        return PerformGetByQuery(query);
    }

    private IEnumerable<IMacro> GetAllNoIds()
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)

            // must be sorted this way for the relator to work
            .OrderBy<MacroDto>(x => x.Id);

        return Database
            .FetchOneToMany<MacroDto>(x => x.MacroPropertyDtos, sql)
            .Transform(ConvertFromDtos)
            .ToArray(); // do it now and once
    }

    private IEnumerable<IMacro> ConvertFromDtos(IEnumerable<MacroDto> dtos)
    {
        foreach (IMacro entity in dtos.Select(x => MacroFactory.BuildEntity(_shortStringHelper, x)))
        {
            // reset dirty initial properties (U4-1946)
            ((BeingDirtyBase)entity).ResetDirtyProperties(false);

            yield return entity;
        }
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
        isCount ? Sql().SelectCount().From<MacroDto>() : GetBaseQuery();

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Macro}.id = @id";

    private Sql<ISqlContext> GetBaseQuery() =>
        Sql()
            .SelectAll()
            .From<MacroDto>()
            .LeftJoin<MacroPropertyDto>()
            .On<MacroDto, MacroPropertyDto>(left => left.Id, right => right.Macro);

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM cmsMacroProperty WHERE macro = @id", "DELETE FROM cmsMacro WHERE id = @id",
        };
        return list;
    }

    protected override void PersistNewItem(IMacro entity)
    {
        entity.AddingEntity();

        MacroDto dto = MacroFactory.BuildDto(entity);

        var id = Convert.ToInt32(Database.Insert(dto));
        entity.Id = id;

        if (dto.MacroPropertyDtos is not null)
        {
            foreach (MacroPropertyDto propDto in dto.MacroPropertyDtos)
            {
                // need to set the id explicitly here
                propDto.Macro = id;
                var propId = Convert.ToInt32(Database.Insert(propDto));
                entity.Properties[propDto.Alias].Id = propId;
            }
        }

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IMacro entity)
    {
        entity.UpdatingEntity();
        MacroDto dto = MacroFactory.BuildDto(entity);

        Database.Update(dto);

        // update the properties if they've changed
        var macro = (Macro)entity;
        if (macro.IsPropertyDirty("Properties") || macro.Properties.Values.Any(x => x.IsDirty()))
        {
            var ids = dto.MacroPropertyDtos?.Where(x => x.Id > 0).Select(x => x.Id).ToArray();
            if (ids?.Length > 0)
            {
                Database.Delete<MacroPropertyDto>("WHERE macro=@macro AND id NOT IN (@ids)", new { macro = dto.Id, ids });
            }
            else
            {
                Database.Delete<MacroPropertyDto>("WHERE macro=@macro", new { macro = dto.Id });
            }

            // detect new aliases, replace with temp aliases
            // this ensures that we don't have collisions, ever
            var aliases = new Dictionary<string, string>();
            if (dto.MacroPropertyDtos is null)
            {
                return;
            }

            foreach (MacroPropertyDto propDto in dto.MacroPropertyDtos)
            {
                IMacroProperty? prop = macro.Properties.Values.FirstOrDefault(x => x.Id == propDto.Id);
                if (prop == null)
                {
                    throw new Exception("oops: property.");
                }

                if (propDto.Id == 0 || prop.IsPropertyDirty("Alias"))
                {
                    var tempAlias = Guid.NewGuid().ToString("N")[..8];
                    aliases[tempAlias] = propDto.Alias;
                    propDto.Alias = tempAlias;
                }
            }

            // insert or update existing properties, with temp aliases
            foreach (MacroPropertyDto propDto in dto.MacroPropertyDtos)
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
                    IMacroProperty? property = macro.Properties.Values.FirstOrDefault(x => x.Id == propDto.Id);
                    if (property == null)
                    {
                        throw new Exception("oops: property.");
                    }

                    if (property.IsDirty())
                    {
                        Database.Update(propDto);
                    }
                }
            }

            // replace the temp aliases with the real ones
            foreach (MacroPropertyDto propDto in dto.MacroPropertyDtos)
            {
                if (aliases.ContainsKey(propDto.Alias) == false)
                {
                    continue;
                }

                propDto.Alias = aliases[propDto.Alias];
                Database.Update(propDto);
            }
        }

        entity.ResetDirtyProperties();
    }
}

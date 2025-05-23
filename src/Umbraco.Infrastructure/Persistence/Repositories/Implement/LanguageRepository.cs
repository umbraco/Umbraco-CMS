using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="Language" />
/// </summary>
internal class LanguageRepository : EntityRepositoryBase<int, ILanguage>, ILanguageRepository
{
    // We need to lock this dictionary every time we do an operation on it as the languageRepository is registered as a unique implementation
    // It is used to quickly get isoCodes by Id, or the reverse by avoiding (deep)cloning dtos
    // It is rebuild on PerformGetAll
    private readonly Dictionary<string, int> _codeIdMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, string> _idCodeMap = new();

    public LanguageRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<LanguageRepository> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    private FullDataSetRepositoryCachePolicy<ILanguage, int>? TypedCachePolicy =>
        CachePolicy as FullDataSetRepositoryCachePolicy<ILanguage, int>;

    public ILanguage? GetByIsoCode(string isoCode)
    {
        EnsureCacheIsPopulated();

        var id = GetIdByIsoCode(isoCode, false);
        return id.HasValue ? Get(id.Value) : null;
    }

    public int? GetIdByIsoCode(string? isoCode, bool throwOnNotFound = true)
    {
        if (isoCode == null)
        {
            return null;
        }

        EnsureCacheIsPopulated();

        lock (_codeIdMap)
        {
            if (_codeIdMap.TryGetValue(isoCode, out var id))
            {
                return id;
            }
        }

        if (throwOnNotFound)
        {
            throw new ArgumentException($"Code {isoCode} does not correspond to an existing language.", nameof(isoCode));
        }

        return null;
    }

    public string? GetIsoCodeById(int? id, bool throwOnNotFound = true)
    {
        if (id == null)
        {
            return null;
        }

        EnsureCacheIsPopulated();

        lock (_codeIdMap)
        {
            if (_idCodeMap.TryGetValue(id.Value, out var isoCode))
            {
                return isoCode;
            }
        }

        if (throwOnNotFound)
        {
            throw new ArgumentException($"Id {id} does not correspond to an existing language.", nameof(id));
        }

        return null;
    }

    // multi implementation of GetIsoCodeById
    public string[] GetIsoCodesByIds(ICollection<int> ids, bool throwOnNotFound = true)
    {
        var isoCodes = new string[ids.Count];

        if (ids.Any() == false)
        {
            return isoCodes;
        }

        EnsureCacheIsPopulated();


        lock (_codeIdMap)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids.ElementAt(i);
                if (_idCodeMap.TryGetValue(id, out var isoCode))
                {
                    isoCodes[i] = isoCode;
                }
                else if (throwOnNotFound)
                {
                    throw new ArgumentException($"Id {id} does not correspond to an existing language.", nameof(id));
                }
            }
        }

        return isoCodes;
    }

    public string GetDefaultIsoCode() => GetDefault().IsoCode;

    public int? GetDefaultId() => GetDefault().Id;

    protected override IRepositoryCachePolicy<ILanguage, int> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<ILanguage, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ false);

    protected ILanguage ConvertFromDto(LanguageDto dto)
    {
        lock (_codeIdMap)
        {
            string? fallbackIsoCode = null;
            if (dto.FallbackLanguageId.HasValue && _idCodeMap.TryGetValue(dto.FallbackLanguageId.Value, out fallbackIsoCode) == false)
            {
                throw new ArgumentException($"The ISO code map did not contain ISO code for fallback language ID: {dto.FallbackLanguageId}. Please reload the caches.");
            }

            return LanguageFactory.BuildEntity(dto, fallbackIsoCode);
        }
    }

    // do NOT leak that language, it's not deep-cloned!
    private ILanguage GetDefault()
    {
        // get all cached
        var languages =
            (TypedCachePolicy
                 ?.GetAllCached(
                     PerformGetAll) // Try to get all cached non-cloned if using the correct cache policy (not the case in unit tests)
             ?? CachePolicy.GetAll(Array.Empty<int>(), PerformGetAll)).ToList();

        ILanguage? language = languages.FirstOrDefault(x => x.IsDefault);
        if (language != null)
        {
            return language;
        }

        // this is an anomaly, the service/repo should ensure it cannot happen
        Logger.LogWarning(
            "There is no default language. Fix this anomaly by editing the language table in database and setting one language as the default language.");

        // still, don't kill the site, and return "something"
        ILanguage? first = null;
        foreach (ILanguage l in languages)
        {
            if (first == null || l.Id < first.Id)
            {
                first = l;
            }
        }

        return first!;
    }

    #region Overrides of RepositoryBase<int,Language>

    protected override ILanguage? PerformGet(int id) => PerformGetAll(id).FirstOrDefault();

    protected override IEnumerable<ILanguage> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<LanguageDto>(x => x.Id > 0);
        if (ids?.Any() ?? false)
        {
            sql.WhereIn<LanguageDto>(x => x.Id, ids);
        }

        // this needs to be sorted since that is the way legacy worked - default language is the first one!!
        // even though legacy didn't sort, it should be by id
        sql.OrderBy<LanguageDto>(x => x.Id);

        // get languages
        List<LanguageDto>? languageDtos = Database.Fetch<LanguageDto>(sql) ?? new List<LanguageDto>();

        // initialize the code-id map if we've reloaded the entire set of languages
        if (ids?.Any() == false)
        {
            lock (_codeIdMap)
            {
                _codeIdMap.Clear();
                _idCodeMap.Clear();
                foreach (LanguageDto languageDto in languageDtos)
                {
                    ArgumentException.ThrowIfNullOrEmpty(languageDto.IsoCode, nameof(LanguageDto.IsoCode));
                    _codeIdMap[languageDto.IsoCode] = languageDto.Id;
                    _idCodeMap[languageDto.Id] = languageDto.IsoCode;
                }
            }
        }

        var languages = languageDtos.Select(ConvertFromDto).OrderBy(x => x.Id).ToList();
        return languages;
    }

    protected override IEnumerable<ILanguage> PerformGetByQuery(IQuery<ILanguage> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<ILanguage>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();
        List<LanguageDto>? dtos = Database.Fetch<LanguageDto>(sql);
        return dtos.Select(ConvertFromDto).ToList();
    }

    #endregion

    #region Overrides of EntityRepositoryBase<int,Language>

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<LanguageDto>();

        sql.From<LanguageDto>();

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Language}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            // NOTE: There is no constraint between the Language and cmsDictionary/cmsLanguageText tables (?)
            // but we still need to remove them
            "DELETE FROM " + Constants.DatabaseSchema.Tables.DictionaryValue + " WHERE languageId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE languageId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersionCultureVariation + " WHERE languageId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.DocumentCultureVariation + " WHERE languageId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.TagRelationship + " WHERE tagId IN (SELECT id FROM " +
            Constants.DatabaseSchema.Tables.Tag + " WHERE languageId = @id)",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Tag + " WHERE languageId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.DocumentUrl + " WHERE languageId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Language + " WHERE id = @id",
        };
        return list;
    }

    #endregion

    #region Unit of Work Implementation

    protected override void PersistNewItem(ILanguage entity)
    {
        // validate iso code and culture name
        if (entity.IsoCode.IsNullOrWhiteSpace() || entity.CultureName.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("Cannot save a language without an ISO code and a culture name.");
        }

        EnsureCacheIsPopulated();

        entity.AddingEntity();

        // deal with entity becoming the new default entity
        if (entity.IsDefault)
        {
            // set all other entities to non-default
            // safe (no race cond) because the service locks languages
            Sql<ISqlContext> setAllDefaultToFalse = Sql()
                .Update<LanguageDto>(u => u.Set(x => x.IsDefault, false));
            Database.Execute(setAllDefaultToFalse);
        }

        // fallback cycles are detected at service level

        // insert
        LanguageDto dto = LanguageFactory.BuildDto(entity, GetFallbackLanguageId(entity));
        var id = Convert.ToInt32(Database.Insert(dto));
        entity.Id = id;
        entity.ResetDirtyProperties();

        // yes, we want to lock _codeIdMap
        lock (_codeIdMap)
        {
            _codeIdMap[entity.IsoCode] = entity.Id;
            _idCodeMap[entity.Id] = entity.IsoCode;
        }
    }

    protected override void PersistUpdatedItem(ILanguage entity)
    {
        // validate iso code and culture name
        if (entity.IsoCode.IsNullOrWhiteSpace() || entity.CultureName.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("Cannot save a language without an ISO code and a culture name.");
        }

        EnsureCacheIsPopulated();

        entity.UpdatingEntity();

        if (entity.IsDefault)
        {
            // deal with entity becoming the new default entity

            // set all other entities to non-default
            // safe (no race cond) because the service locks languages
            Sql<ISqlContext> setAllDefaultToFalse = Sql()
                .Update<LanguageDto>(u => u.Set(x => x.IsDefault, false));
            Database.Execute(setAllDefaultToFalse);
        }
        else
        {
            // deal with the entity not being default anymore
            // which is illegal - another entity has to become default
            Sql<ISqlContext> selectDefaultId = Sql()
                .Select<LanguageDto>(x => x.Id)
                .From<LanguageDto>()
                .Where<LanguageDto>(x => x.IsDefault);

            var defaultId = Database.ExecuteScalar<int>(selectDefaultId);
            if (entity.Id == defaultId)
            {
                throw new InvalidOperationException(
                    $"Cannot save the default language ({entity.IsoCode}) as non-default. Make another language the default language instead.");
            }
        }

        if (entity.IsPropertyDirty(nameof(ILanguage.IsoCode)))
        {
            // If the iso code is changing, ensure there's not another lang with the same code already assigned
            Sql<ISqlContext> sameCode = Sql()
                .SelectCount()
                .From<LanguageDto>()
                .Where<LanguageDto>(x => x.IsoCode == entity.IsoCode && x.Id != entity.Id);

            var countOfSameCode = Database.ExecuteScalar<int>(sameCode);
            if (countOfSameCode > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot update the language to a new culture: {entity.IsoCode} since that culture is already assigned to another language entity.");
            }
        }

        // fallback cycles are detected at service level

        // update
        LanguageDto dto = LanguageFactory.BuildDto(entity, GetFallbackLanguageId(entity));
        Database.Update(dto);
        entity.ResetDirtyProperties();

        // yes, we want to lock _codeIdMap
        lock (_codeIdMap)
        {
            _codeIdMap.RemoveAll(kvp => kvp.Value == entity.Id);
            _codeIdMap[entity.IsoCode] = entity.Id;
            _idCodeMap[entity.Id] = entity.IsoCode;
        }
    }

    protected override void PersistDeletedItem(ILanguage entity)
    {
        // validate that the entity is not the default language.
        // safe (no race cond) because the service locks languages
        Sql<ISqlContext> selectDefaultId = Sql()
            .Select<LanguageDto>(x => x.Id)
            .From<LanguageDto>()
            .Where<LanguageDto>(x => x.IsDefault);

        var defaultId = Database.ExecuteScalar<int>(selectDefaultId);
        if (entity.Id == defaultId)
        {
            throw new InvalidOperationException($"Cannot delete the default language ({entity.IsoCode}).");
        }

        // We need to remove any references to the language if it's being used as a fall-back from other ones
        Sql<ISqlContext> clearFallbackLanguage = Sql()
            .Update<LanguageDto>(u => u
                .Set(x => x.FallbackLanguageId, null))
            .Where<LanguageDto>(x => x.FallbackLanguageId == entity.Id);

        Database.Execute(clearFallbackLanguage);

        // delete
        base.PersistDeletedItem(entity);

        // yes, we want to lock _codeIdMap
        lock (_codeIdMap)
        {
            _codeIdMap.RemoveAll(kvp => kvp.Value == entity.Id);
            _idCodeMap.Remove(entity.Id);
        }
    }

    private void EnsureCacheIsPopulated()
    {
        // ensure cache is populated, in a non-expensive way
        if (TypedCachePolicy != null)
        {
            TypedCachePolicy.GetAllCached(PerformGetAll);
        }
        else
        {
            PerformGetAll(); // We don't have a typed cache (i.e. unit tests) but need to populate the _codeIdMap
        }
    }

    private int? GetFallbackLanguageId(ILanguage entity)
    {
        int? fallbackLanguageId = null;
        if (entity.FallbackIsoCode.IsNullOrWhiteSpace() == false &&
            _codeIdMap.TryGetValue(entity.FallbackIsoCode, out var languageId))
        {
            fallbackLanguageId = languageId;
        }

        return fallbackLanguageId;
    }

    #endregion
}

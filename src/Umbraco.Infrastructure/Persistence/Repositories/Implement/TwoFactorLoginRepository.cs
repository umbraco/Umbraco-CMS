using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class TwoFactorLoginRepository : EntityRepositoryBase<int, ITwoFactorLogin>, ITwoFactorLoginRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TwoFactorLoginRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the database scope for transactional operations.</param>
    /// <param name="cache">The application-level caches used for performance optimization.</param>
    /// <param name="logger">The logger used for logging repository operations.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    public TwoFactorLoginRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<TwoFactorLoginRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    /// <summary>
    /// Asynchronously deletes all two-factor login records associated with the specified user or member key.
    /// </summary>
    /// <param name="userOrMemberKey">The unique identifier of the user or member whose two-factor logins will be deleted.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if any logins were deleted; otherwise, <c>false</c>.</returns>
    public async Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey) =>
        await DeleteUserLoginsAsync(userOrMemberKey, null);

    /// <summary>
    /// Deletes the two-factor login records for a specified user or member.
    /// </summary>
    /// <param name="userOrMemberKey">The unique identifier of the user or member.</param>
    /// <param name="providerName">The name of the two-factor login provider. If null, all providers are deleted.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the deletion was successful; otherwise, false.</returns>
    public async Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey, string? providerName)
    {
        Sql<ISqlContext> sql = Sql()
            .Delete()
            .From<TwoFactorLoginDto>()
            .Where<TwoFactorLoginDto>(x => x.UserOrMemberKey == userOrMemberKey);

        if (providerName is not null)
        {
            sql = sql.Where<TwoFactorLoginDto>(x => x.ProviderName == providerName);
        }

        var deletedRows = await Database.ExecuteAsync(sql);

        return deletedRows > 0;
    }

    /// <summary>
    /// Asynchronously retrieves all two-factor login entries associated with the specified user or member key.
    /// </summary>
    /// <param name="userOrMemberKey">The unique identifier (GUID) of the user or member whose two-factor login entries are to be retrieved.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of <see cref="ITwoFactorLogin"/> entries associated with the specified key.
    /// </returns>
    public async Task<IEnumerable<ITwoFactorLogin>> GetByUserOrMemberKeyAsync(Guid userOrMemberKey)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<TwoFactorLoginDto>()
            .From<TwoFactorLoginDto>()
            .Where<TwoFactorLoginDto>(x => x.UserOrMemberKey == userOrMemberKey);
        List<TwoFactorLoginDto>? dtos = await Database.FetchAsync<TwoFactorLoginDto>(sql);
        return dtos.WhereNotNull().Select(Map).WhereNotNull();
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<TwoFactorLoginDto>();

        sql.From<TwoFactorLoginDto>();

        return sql;
    }

    protected override string GetBaseWhereClause() =>
        QuoteTableName(Constants.DatabaseSchema.Tables.TwoFactorLogin) + ".id = @id";

    protected override IEnumerable<string> GetDeleteClauses() => Enumerable.Empty<string>();

    protected override ITwoFactorLogin? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<TwoFactorLoginDto>(x => x.Id == id);
        TwoFactorLoginDto? dto = Database.Fetch<TwoFactorLoginDto>(sql).FirstOrDefault();
        return dto == null ? null : Map(dto);
    }

    protected override IEnumerable<ITwoFactorLogin> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).WhereIn<TwoFactorLoginDto>(x => x.Id, ids);
        List<TwoFactorLoginDto>? dtos = Database.Fetch<TwoFactorLoginDto>(sql);
        return dtos.WhereNotNull().Select(Map).WhereNotNull();
    }

    protected override IEnumerable<ITwoFactorLogin> PerformGetByQuery(IQuery<ITwoFactorLogin> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<ITwoFactorLogin>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();
        return Database.Fetch<TwoFactorLoginDto>(sql).Select(Map).WhereNotNull();
    }

    protected override void PersistNewItem(ITwoFactorLogin entity)
    {
        TwoFactorLoginDto? dto = Map(entity);
        Database.Insert(dto);
    }

    protected override void PersistUpdatedItem(ITwoFactorLogin entity)
    {
        TwoFactorLoginDto? dto = Map(entity);
        if (dto is not null)
        {
            Database.Update(dto);
        }
    }

    private static TwoFactorLoginDto? Map(ITwoFactorLogin entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new TwoFactorLoginDto
        {
            Id = entity.Id,
            UserOrMemberKey = entity.UserOrMemberKey,
            ProviderName = entity.ProviderName,
            Secret = entity.Secret
        };
    }

    private static ITwoFactorLogin? Map(TwoFactorLoginDto dto)
    {
        if (dto == null)
        {
            return null;
        }

        return new TwoFactorLogin
        {
            Id = dto.Id,
            UserOrMemberKey = dto.UserOrMemberKey,
            ProviderName = dto.ProviderName,
            Secret = dto.Secret
        };
    }
}

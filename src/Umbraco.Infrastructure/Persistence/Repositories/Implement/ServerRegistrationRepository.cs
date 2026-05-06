using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class ServerRegistrationRepository : EntityRepositoryBase<int, IServerRegistration>,
    IServerRegistrationRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerRegistrationRepository"/> class, responsible for managing server registration data in the database.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="logger">The logger used for logging repository-related events and errors.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning of repository data.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed servers.</param>
    public ServerRegistrationRepository(
        IScopeAccessor scopeAccessor,
        ILogger<ServerRegistrationRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            AppCaches.NoCache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    /// <summary>
    /// Clears the cache used by the server registration repository.
    /// </summary>
    public void ClearCache() => CachePolicy.ClearAll();

    /// <summary>
    /// Deactivates all server registrations that have not accessed the system within the specified timeout period.
    /// </summary>
    /// <param name="staleTimeout">The duration after which a server is considered stale and will be deactivated.</param>
    public void DeactiveStaleServers(TimeSpan staleTimeout)
    {
        DateTime timeoutDate = DateTime.UtcNow.Subtract(staleTimeout);

        Sql<ISqlContext> sql = Sql()
            .Update<ServerRegistrationDto>(c => c
                .Set(x => x.IsActive, false)
                .Set(x => x.IsSchedulingPublisher, false))
            .Where<ServerRegistrationDto>(x => x.DateAccessed < timeoutDate);
        Database.Execute(sql);

        ClearCache();
    }

    protected override IRepositoryCachePolicy<IServerRegistration, int> CreateCachePolicy() =>

        // TODO: what are we doing with cache here?
        // why are we using disabled cache helper up there?
        //
        // 7.6 says:
        // note: this means that the ServerRegistrationRepository does *not* implement scoped cache,
        // and this is because the repository is special and should not participate in scopes
        // (cleanup in v8)
        new FullDataSetRepositoryCachePolicy<IServerRegistration, int>(AppCaches.RuntimeCache, ScopeAccessor,  RepositoryCacheVersionService, CacheSyncService, GetEntityId, /*expires:*/ false);

    protected override int PerformCount(IQuery<IServerRegistration>? query) =>
        throw new NotSupportedException("This repository does not support this method.");

    protected override bool PerformExists(int id) =>

        // use the underlying GetAll which force-caches all registrations
        GetMany().Any(x => x.Id == id);

    protected override IServerRegistration? PerformGet(int id) =>

        // use the underlying GetAll which force-caches all registrations
        GetMany().FirstOrDefault(x => x.Id == id);

    protected override IEnumerable<IServerRegistration> PerformGetAll(params int[]? ids) =>
        Database.Fetch<ServerRegistrationDto>("WHERE id > 0")
            .Select(x => ServerRegistrationFactory.BuildEntity(x));

    protected override IEnumerable<IServerRegistration> PerformGetByQuery(IQuery<IServerRegistration> query) =>
        throw new NotSupportedException("This repository does not support this method.");

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<ServerRegistrationDto>();

        sql
            .From<ServerRegistrationDto>();

        return sql;
    }

    protected override string GetBaseWhereClause() => "id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string> { $"DELETE FROM {QuoteTableName("umbracoServer")} WHERE id = @id" };
        return list;
    }

    protected override void PersistNewItem(IServerRegistration entity)
    {
        entity.AddingEntity();

        ServerRegistrationDto dto = ServerRegistrationFactory.BuildDto(entity);

        var id = Convert.ToInt32(Database.Insert(dto));
        entity.Id = id;

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IServerRegistration entity)
    {
        entity.UpdatingEntity();

        ServerRegistrationDto dto = ServerRegistrationFactory.BuildDto(entity);

        Database.Update(dto);

        entity.ResetDirtyProperties();
    }
}

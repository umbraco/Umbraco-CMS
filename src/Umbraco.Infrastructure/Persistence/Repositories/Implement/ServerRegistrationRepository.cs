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

internal class ServerRegistrationRepository : EntityRepositoryBase<int, IServerRegistration>,
    IServerRegistrationRepository
{
    public ServerRegistrationRepository(IScopeAccessor scopeAccessor, ILogger<ServerRegistrationRepository> logger)
        : base(scopeAccessor, AppCaches.NoCache, logger)
    {
    }

    public void ClearCache() => CachePolicy.ClearAll();

    public void DeactiveStaleServers(TimeSpan staleTimeout)
    {
        DateTime timeoutDate = DateTime.Now.Subtract(staleTimeout);

        Database.Update<ServerRegistrationDto>(
            "SET isActive=0, isSchedulingPublisher=0 WHERE lastNotifiedDate < @timeoutDate", new
            {
                /*timeoutDate =*/
                timeoutDate,
            });
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
        new FullDataSetRepositoryCachePolicy<IServerRegistration, int>(AppCaches.RuntimeCache, ScopeAccessor, GetEntityId, /*expires:*/ false);

    protected override int PerformCount(IQuery<IServerRegistration> query) =>
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
        var list = new List<string> { "DELETE FROM umbracoServer WHERE id = @id" };
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

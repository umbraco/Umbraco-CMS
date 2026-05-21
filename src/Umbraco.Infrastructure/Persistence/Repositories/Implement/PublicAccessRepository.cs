using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Cache;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class PublicAccessRepository : AsyncEntityRepositoryBase<Guid, PublicAccessEntry>, IPublicAccessRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="cache">The application-level caches used for optimizing data retrieval.</param>
    /// <param name="logger">The logger used for logging repository events and errors.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning within the repository.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public PublicAccessRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<PublicAccessRepository> logger,
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

    /// <inheritdoc/>
    protected override IAsyncRepositoryCachePolicy<PublicAccessEntry, Guid> CreateCachePolicy() =>
        new AsyncFullDataSetRepositoryCachePolicy<PublicAccessEntry, Guid>(GlobalIsolatedCache, ScopeAccessor,  RepositoryCacheVersionService, CacheSyncService, GetEntityKey, /*expires:*/ false);

    /// <inheritdoc/>
    protected override async Task<PublicAccessEntry?> PerformGetAsync(Guid id) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            AccessDto? dto = await db.Access
                .Include(x => x.Rules)
                .FirstOrDefaultAsync(x => x.Id == id);

            return dto is null ? null : PublicAccessEntryFactory.BuildEntity(dto);
        });


    /// <inheritdoc/>
    protected override async Task<IEnumerable<PublicAccessEntry>?> PerformGetManyAsync(Guid[]? keys) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            AccessDto[] dtos = await db.Access
                .Include(x => x.Rules)
                .Where(x => keys!.Contains(x.Id))
                .OrderBy(x => x.NodeId)
                .ToArrayAsync();

            return dtos.Select(PublicAccessEntryFactory.BuildEntity);
        });

    /// <inheritdoc/>
    protected override async Task<IEnumerable<PublicAccessEntry>?> PerformGetAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            AccessDto[] dtos = await db.Access
                .Include(x => x.Rules)
                .OrderBy(x => x.NodeId)
                .ToArrayAsync();

            return dtos.Select(PublicAccessEntryFactory.BuildEntity);
        });


    /// <inheritdoc/>
    protected override async Task PersistNewItemAsync(PublicAccessEntry entity)
    {
        entity.AddingEntity();
        foreach (PublicAccessRule rule in entity.Rules)
        {
            rule.AddingEntity();
        }

        AccessDto dto = PublicAccessEntryFactory.BuildDto(entity);

        await AmbientScope.ExecuteWithContextAsync<AccessDto>(async db =>
        {
            db.Access.Add(dto);

            // update the id so HasEntity is correct
            entity.Id = entity.Key.GetHashCode();

            foreach (AccessRuleDto rule in dto.Rules)
            {
                rule.AccessId = entity.Key;
                db.AccessRules.Add(rule);
            }

            // update the id so HasEntity is correct
            foreach (PublicAccessRule rule in entity.Rules)
            {
                rule.Id = rule.Key.GetHashCode();
            }

            await db.SaveChangesAsync();
            entity.ResetDirtyProperties();
        });
    }

    /// <inheritdoc/>
    protected override async Task PersistUpdatedItemAsync(PublicAccessEntry entity)
    {
        entity.UpdatingEntity();
        foreach (PublicAccessRule rule in entity.Rules)
        {
            if (rule.HasIdentity)
            {
                rule.UpdatingEntity();
            }
            else
            {
                rule.AddingEntity();
            }
        }

        AccessDto dto = PublicAccessEntryFactory.BuildDto(entity);

        await AmbientScope.ExecuteWithContextAsync<AccessDto>(async db =>
        {
            await db.Access
                .Where(x => x.Id == dto.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(x => x.NodeId, dto.NodeId)
                    .SetProperty(x => x.LoginNodeId, dto.LoginNodeId)
                    .SetProperty(x => x.NoAccessNodeId, dto.NoAccessNodeId)
                    .SetProperty(x => x.CreateDate, dto.CreateDate)
                    .SetProperty(x => x.UpdateDate, dto.UpdateDate));

            foreach (Guid removedRule in entity.RemovedRules)
            {
                await db.AccessRules
                    .Where(x => x.Id == removedRule)
                    .ExecuteDeleteAsync();
            }

            foreach (PublicAccessRule rule in entity.Rules)
            {
                AccessRuleDto ruleDto = dto.Rules.Single(x => x.Id == rule.Key);

                if (rule.HasIdentity)
                {
                    await db.AccessRules
                        .Where(x => x.Id == ruleDto.Id)
                        .ExecuteUpdateAsync(setter => setter
                            .SetProperty(x => x.RuleValue, ruleDto.RuleValue)
                            .SetProperty(x => x.RuleType, ruleDto.RuleType)
                            .SetProperty(x => x.UpdateDate, ruleDto.UpdateDate));
                }
                else
                {
                    db.AccessRules.Add(ruleDto);
                    await db.SaveChangesAsync();

                    // update the id so HasEntity is correct
                    rule.Id = rule.Key.GetHashCode();
                }
            }

            entity.ResetDirtyProperties();
        });
    }

    /// <inheritdoc/>
    protected override async Task PersistDeletedItemAsync(PublicAccessEntry entity) =>
        await AmbientScope.ExecuteWithContextAsync<AccessDto>(async db =>
        {
            await db.AccessRules
                .Where(x => x.AccessId == entity.Key)
                .ExecuteDeleteAsync();

            await db.Access
                .Where(x => x.Id == entity.Key)
                .ExecuteDeleteAsync();
        });

    /// <inheritdoc/>
    protected override Guid GetEntityKey(PublicAccessEntry entity) => entity.Key;
}

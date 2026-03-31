using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
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

internal sealed class DomainRepository : AsyncEntityRepositoryBase<Guid, IDomain>, IDomainRepository
{
    public DomainRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<DomainRepository> logger,
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
    public async Task<IDomain?> GetByNameAsync(string domainName)
    {
        IEnumerable<IDomain> all = await GetAllAsync(CancellationToken.None);
        return all.FirstOrDefault(x => x.DomainName.InvariantEquals(domainName));
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string domainName)
    {
        IEnumerable<IDomain> all = await GetAllAsync(CancellationToken.None);
        return all.Any(x => x.DomainName.InvariantEquals(domainName));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IDomain>> GetAllAsync(bool includeWildcards)
    {
        IEnumerable<IDomain> all = await GetAllAsync(CancellationToken.None);
        return all.Where(x => includeWildcards || x.IsWildcard == false);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IDomain>> GetAssignedDomainsAsync(Guid contentKey, bool includeWildcards)
    {
        IEnumerable<IDomain> all = await GetAllAsync(CancellationToken.None);
        return all
            .Where(x => x.RootContentKey == contentKey)
            .Where(x => includeWildcards || x.IsWildcard == false);
    }

    /// <inheritdoc/>
    protected override IAsyncRepositoryCachePolicy<IDomain, Guid> CreateCachePolicy()
        => new AsyncFullDataSetRepositoryCachePolicy<IDomain, Guid>(GlobalIsolatedCache, ScopeAccessor, RepositoryCacheVersionService, CacheSyncService, entity => entity.Key, false);

    /// <inheritdoc/>
    protected override async Task<IDomain?> PerformGetAsync(Guid key)
    {
        // Use the underlying GetAll which will force cache all domains
        IEnumerable<IDomain> all = await GetAllAsync(CancellationToken.None);
        return all.FirstOrDefault(x => x.Key == key);
    }

    /// <inheritdoc/>
    protected override async Task<IEnumerable<IDomain>?> PerformGetAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<DomainDto> dtos = await db.Domains
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return await BuildEntitiesAsync(db, dtos);
        });

    /// <inheritdoc/>
    protected override async Task<IEnumerable<IDomain>?> PerformGetManyAsync(Guid[]? keys)
    {
        if (keys is null || keys.Length == 0)
        {
            return null;
        }

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<DomainDto> dtos = await db.Domains
                .Where(x => keys.Contains(x.Key))
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return await BuildEntitiesAsync(db, dtos);
        });
    }

    /// <inheritdoc/>
    protected override async Task PersistNewItemAsync(IDomain entity) =>
        await AmbientScope.ExecuteWithContextAsync<DomainDto>(async db =>
        {
            // Check duplicate domain name
            var exists = await db.Domains.AnyAsync(x => x.DomainName == entity.DomainName);
            if (exists)
            {
                throw new DuplicateNameException($"The domain name {entity.DomainName} is already assigned.");
            }

            // Validate RootContentId exists
            if (entity.RootContentId.HasValue)
            {
                var contentExists = await db.Nodes.AnyAsync(x => x.NodeId == entity.RootContentId.Value);
                if (!contentExists)
                {
                    throw new NullReferenceException($"No content exists with id {entity.RootContentId.Value}.");
                }
            }

            // Validate LanguageId exists
            if (entity.LanguageId.HasValue)
            {
                var languageExists = await db.Language.AnyAsync(x => x.Id == entity.LanguageId.Value);
                if (!languageExists)
                {
                    throw new NullReferenceException($"No language exists with id {entity.LanguageId.Value}.");
                }
            }

            entity.AddingEntity();
            entity.SortOrder = await GetNewSortOrderAsync(db, entity.RootContentId, entity.IsWildcard);

            DomainDto dto = DomainFactory.BuildDto(entity);
            db.Domains.Add(dto);
            await db.SaveChangesAsync();

            entity.Id = dto.Id;

            // Resolve ISO code
            if (entity.LanguageId.HasValue)
            {
                LanguageDto? lang = await db.Language.FirstOrDefaultAsync(x => x.Id == entity.LanguageId.Value);
                ((UmbracoDomain)entity).LanguageIsoCode = lang?.IsoCode;
            }

            entity.ResetDirtyProperties();
        });

    /// <inheritdoc/>
    protected override async Task PersistUpdatedItemAsync(IDomain entity) =>
        await AmbientScope.ExecuteWithContextAsync<DomainDto>(async db =>
        {
            entity.UpdatingEntity();

            // Check duplicate domain name (excluding self)
            var exists = await db.Domains.AnyAsync(x => x.DomainName == entity.DomainName && x.Id != entity.Id);
            if (exists)
            {
                throw new DuplicateNameException($"The domain name {entity.DomainName} is already assigned.");
            }

            // Validate RootContentId exists
            if (entity.RootContentId.HasValue)
            {
                var contentExists = await db.Nodes.AnyAsync(x => x.NodeId == entity.RootContentId.Value);
                if (!contentExists)
                {
                    throw new NullReferenceException($"No content exists with id {entity.RootContentId.Value}.");
                }
            }

            // Validate LanguageId exists
            if (entity.LanguageId.HasValue)
            {
                var languageExists = await db.Language.AnyAsync(x => x.Id == entity.LanguageId.Value);
                if (!languageExists)
                {
                    throw new NullReferenceException($"No language exists with id {entity.LanguageId.Value}.");
                }
            }

            DomainDto dto = DomainFactory.BuildDto(entity);

            await db.Domains
                .Where(x => x.Id == dto.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(x => x.Key, dto.Key)
                    .SetProperty(x => x.DomainName, dto.DomainName)
                    .SetProperty(x => x.DefaultLanguage, dto.DefaultLanguage)
                    .SetProperty(x => x.RootStructureId, dto.RootStructureId)
                    .SetProperty(x => x.SortOrder, dto.SortOrder));

            // Resolve ISO code if language changed
            if (entity.WasPropertyDirty("LanguageId"))
            {
                LanguageDto? lang = await db.Language.FirstOrDefaultAsync(x => x.Id == entity.LanguageId);
                ((UmbracoDomain)entity).LanguageIsoCode = lang?.IsoCode;
            }

            entity.ResetDirtyProperties();
        });

    /// <inheritdoc/>
    protected override async Task PersistDeletedItemAsync(IDomain entity)
    {
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            await db.Domains.Where(x => x.Id == entity.Id).ExecuteDeleteAsync();
            return true;
        });
    }

    /// <inheritdoc/>
    protected override Task<bool> PerformExistsAsync(Guid key) =>
        AmbientScope.ExecuteWithContextAsync(db => db.Domains.AnyAsync(x => x.Key == key));

    private static async Task<IEnumerable<IDomain>> BuildEntitiesAsync(UmbracoDbContext db, List<DomainDto> dtos)
    {
        // Batch-load language ISO codes
        var languageIds = dtos
            .Where(d => d.DefaultLanguage.HasValue)
            .Select(d => d.DefaultLanguage!.Value)
            .Distinct()
            .ToList();

        Dictionary<int, string> isoCodeLookup = await db.Language
            .Where(l => languageIds.Contains(l.Id))
            .Select(l => new { l.Id, l.IsoCode })
            .ToDictionaryAsync(l => l.Id, l => l.IsoCode!);

        var nodeIds = dtos
            .Where(d => d.RootStructureId.HasValue)
            .Select(d => d.RootStructureId!.Value)
            .Distinct()
            .ToList();

        Dictionary<int, Guid> nodeKeyLookup = await db.Nodes
            .Where(n => nodeIds.Contains(n.NodeId))
            .Select(n => new { n.NodeId, n.UniqueId })
            .ToDictionaryAsync(n => n.NodeId, n => n.UniqueId);

        return DomainFactory.BuildEntities(dtos, isoCodeLookup, nodeKeyLookup);
    }

    private static async Task<int> GetNewSortOrderAsync(UmbracoDbContext db, int? rootContentId, bool isWildcard)
    {
        if (isWildcard)
        {
            return -1;
        }

        var maxSortOrder = await db.Domains
            .Where(x => x.RootStructureId == rootContentId)
            .Where(x => x.DomainName != string.Empty && !x.DomainName.StartsWith("*"))
            .MaxAsync(x => (int?)x.SortOrder) ?? -1;

        return maxSortOrder + 1;
    }

}

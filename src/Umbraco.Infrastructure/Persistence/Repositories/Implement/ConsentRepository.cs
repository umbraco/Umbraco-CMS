using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the NPoco implementation of <see cref="IConsentRepository" />.
/// </summary>
internal sealed class ConsentRepository : AsyncEntityRepositoryBase<int, IConsent>, IConsentRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConsentRepository" /> class.
    /// </summary>
    public ConsentRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<ConsentRepository> logger,
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

    /// <inheritdoc />
    public async Task ClearCurrentAsync(string source, string context, string action) =>
        await AmbientScope.ExecuteWithContextAsync<ConsentDto>(async db =>
        {
            await db.Consents
                .Where(x => x.Source == source && x.Context == context && x.Action == action && x.Current)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.Current, false));
        });

    /// <inheritdoc />
    public async Task<IEnumerable<IConsent>> LookupAsync(
        string? source = null,
        string? context = null,
        string? action = null,
        bool sourceStartsWith = false,
        bool contextStartsWith = false,
        bool actionStartsWith = false,
        bool includeHistory = false)
    {
        return await AmbientScope.ExecuteWithContextAsync<IEnumerable<IConsent>>(async db =>
        {
            IQueryable<ConsentDto> query = db.Consents.AsQueryable();

            if (string.IsNullOrWhiteSpace(source) == false)
            {
                query = sourceStartsWith
                    ? query.Where(x => x.Source != null && x.Source.StartsWith(source))
                    : query.Where(x => x.Source == source);
            }

            if (string.IsNullOrWhiteSpace(context) == false)
            {
                query = contextStartsWith
                    ? query.Where(x => x.Context != null && x.Context.StartsWith(context))
                    : query.Where(x => x.Context == context);
            }

            if (string.IsNullOrWhiteSpace(action) == false)
            {
                query = actionStartsWith
                    ? query.Where(x => x.Action != null && x.Action.StartsWith(action))
                    : query.Where(x => x.Action == action);
            }

            if (includeHistory == false)
            {
                query = query.Where(x => x.Current);
            }

            List<ConsentDto> dtos = await query.OrderByDescending(x => x.CreateDate).ToListAsync();
            return ConsentFactory.BuildEntities(dtos);
        });
    }

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(IConsent entity)
    {
        entity.AddingEntity();

        ConsentDto dto = ConsentFactory.BuildDto(entity);

        await AmbientScope.ExecuteWithContextAsync<ConsentDto>(async db =>
        {
            db.Consents.Add(dto);

            await db.SaveChangesAsync();
        });

        entity.Id = dto.Id;
        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override async Task PersistUpdatedItemAsync(IConsent entity)
    {
        entity.UpdatingEntity();

        ConsentDto dto = ConsentFactory.BuildDto(entity);

        await AmbientScope.ExecuteWithContextAsync<ConsentDto>(async db =>
        {
            db.Consents.Update(dto);

            await db.SaveChangesAsync();
        });

        entity.ResetDirtyProperties();
        IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IConsent, int>(entity.Id));
    }

    /// <inheritdoc />
    protected override Task<IConsent?> PerformGetAsync(int id) => throw new NotSupportedException();

    /// <inheritdoc />
    protected override Task<IEnumerable<IConsent>?> PerformGetAllAsync() => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override Task<IEnumerable<IConsent>?> PerformGetManyAsync(int[]? keys) => throw new NotSupportedException();
}

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class RedirectUrlRepository : AsyncEntityRepositoryBase<Guid, IRedirectUrl>, IRedirectUrlRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectUrlRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="cache">The application-level caches used for storing and retrieving cached data.</param>
    /// <param name="logger">The logger used for logging repository events and errors.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning within the repository.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public RedirectUrlRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<RedirectUrlRepository> logger,
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
    /// Retrieves a redirect URL entry that matches the specified URL, content key, and optional culture.
    /// </summary>
    /// <param name="url">The URL to match.</param>
    /// <param name="contentKey">The unique key of the content associated with the redirect URL.</param>
    /// <param name="culture">The culture associated with the redirect URL, or <c>null</c> to match entries without a culture.</param>
    /// <returns>The matching <see cref="IRedirectUrl"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<IRedirectUrl?> GetAsync(string url, Guid contentKey, string? culture)
    {
        var urlHash = url.GenerateHash<SHA1>();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var result = await db.RedirectUrls
                .Where(x => x.Url == url
                            && x.UrlHash == urlHash
                            && x.ContentKey == contentKey
                            && x.Culture == culture)
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId })
                .FirstOrDefaultAsync();

            if (result is null)
            {
                return null;
            }

            return Map(result.Redirect, result.ContentId);
        });
    }

    /// <summary>
    /// Deletes all redirect URLs from the database.
    /// </summary>
    public async Task DeleteAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync<RedirectUrlDto>(async db =>
        {
            await db.RedirectUrls.ExecuteDeleteAsync();
        });

    /// <summary>
    /// Deletes all redirect URLs associated with the specified content key.
    /// </summary>
    /// <param name="contentKey">The unique identifier of the content whose redirect URLs should be deleted.</param>
    public async Task DeleteContentUrlsAsync(Guid contentKey) =>
        await AmbientScope.ExecuteWithContextAsync<RedirectUrlDto>(async db =>
        {
            await db.RedirectUrls
                .Where(x => x.ContentKey == contentKey)
                .ExecuteDeleteAsync();
        });

    /// <summary>
    /// Deletes the redirect URL entry identified by the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the redirect URL entry to delete.</param>

    public async Task DeleteAsync(Guid id) =>
        await AmbientScope.ExecuteWithContextAsync<RedirectUrlDto>(async db =>
        {
            await db.RedirectUrls
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();
        });

    /// <summary>
    /// Gets the most recent redirect URL matching the specified URL.
    /// </summary>
    /// <param name="url">The URL to find the most recent redirect for.</param>
    /// <returns>The most recent <see cref="Umbraco.Cms.Core.Models.IRedirectUrl"/> if found; otherwise, null.</returns>
    public async Task<IRedirectUrl?> GetMostRecentUrlAsync(string url)
    {
        var urlHash = url.GenerateHash<SHA1>();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var result = await db.RedirectUrls
                .Where(x => x.Url == url && x.UrlHash == urlHash)
                .OrderByDescending(x => x.CreateDateUtc)
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId })
                .FirstOrDefaultAsync();

            return result == null ? null : Map(result.Redirect, result.ContentId);
        });
    }

    /// <inheritdoc/>
    public async Task<IRedirectUrl?> GetMostRecentUrlAsync(string url, string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return await GetMostRecentUrlAsync(url);
        }

        var urlHash = url.GenerateHash<SHA1>();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var results = await db.RedirectUrls
                .Where(x => x.Url == url
                            && x.UrlHash == urlHash
                            && (x.Culture == culture.ToLower()
                                || x.Culture == null
                                || x.Culture == string.Empty))
                .OrderByDescending(x => x.CreateDateUtc)
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId })
                .ToListAsync();

            var match = results.FirstOrDefault(f => culture.InvariantEquals(f.Redirect.Culture))
                        ?? results.FirstOrDefault(f => string.IsNullOrWhiteSpace(f.Redirect.Culture));

            return match is null ? null : Map(match.Redirect, match.ContentId);
        });
    }

    /// <summary>
    /// Retrieves all redirect URLs that are associated with the specified content item.
    /// </summary>
    /// <param name="contentKey">The unique key (GUID) identifying the content item.</param>
    /// <returns>An enumerable collection of <see cref="IRedirectUrl"/> instances representing the redirect URLs for the specified content.</returns>
    public async Task<IEnumerable<IRedirectUrl>> GetContentUrlsAsync(Guid contentKey) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var contentId = await db.Nodes
                .Where(n => n.UniqueId == contentKey)
                .Select(n => n.NodeId)
                .FirstOrDefaultAsync();

            List<RedirectUrlDto> dtos = await db.RedirectUrls
                .Where(x => x.ContentKey == contentKey)
                .OrderByDescending(x => x.CreateDateUtc)
                .ToListAsync();

            return dtos.Select(dto => Map(dto, contentId));
        });

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> GetAllUrlsAsync(long pageIndex, int pageSize) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var total = await db.RedirectUrls.LongCountAsync();

            var results = await db.RedirectUrls
                .OrderByDescending(x => x.CreateDateUtc)
                .Skip((int)(pageIndex * pageSize))
                .Take(pageSize)
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId })
                .ToListAsync();

            IEnumerable<IRedirectUrl> items = results.Select(r => Map(r.Redirect, r.ContentId));
            return new PagedModel<IRedirectUrl>(total, items);
        });

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> GetAllUrlsAsync(int rootContentId, long pageIndex, int pageSize) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var pathPattern = "," + rootContentId + ",";

            var query = db.RedirectUrls
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId, node.Path })
                .Where(x => x.Path.Contains(pathPattern));

            var total = await query.LongCountAsync();

            var results = await query
                .OrderByDescending(x => x.Redirect.CreateDateUtc)
                .Skip((int)(pageIndex * pageSize))
                .Take(pageSize)
                .ToListAsync();

            IEnumerable<IRedirectUrl> items = results.Select(r => Map(r.Redirect, r.ContentId));
            return new PagedModel<IRedirectUrl>(total, items);
        });

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> SearchUrlsAsync(string searchTerm, long pageIndex, int pageSize) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var term = searchTerm.Trim().ToLowerInvariant();

            var total = await db.RedirectUrls
                .Where(x => x.Url.Contains(term))
                .LongCountAsync();

            var results = await db.RedirectUrls
                .Where(x => x.Url.Contains(term))
                .OrderByDescending(x => x.CreateDateUtc)
                .Skip((int)(pageIndex * pageSize))
                .Take(pageSize)
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId })
                .ToListAsync();

            IEnumerable<IRedirectUrl> items = results.Select(r => Map(r.Redirect, r.ContentId));
            return new PagedModel<IRedirectUrl>(total, items);
        });

    /// <inheritdoc/>
    protected override async Task<bool> PerformExistsAsync(Guid id) => await PerformGetAsync(id) != null;

    /// <inheritdoc/>
    protected override async Task<IRedirectUrl?> PerformGetAsync(Guid id) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var result = await db.RedirectUrls
                .Where(x => x.Id == id)
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId })
                .FirstOrDefaultAsync();

            return result == null ? null : Map(result.Redirect, result.ContentId);
        });

    /// <inheritdoc/>
    protected override async Task<IEnumerable<IRedirectUrl>?> PerformGetAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var results = await db.RedirectUrls
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId })
                .ToListAsync();

            return results.Select(r => Map(r.Redirect, r.ContentId));
        });

    /// <inheritdoc/>
    protected override async Task<IEnumerable<IRedirectUrl>?> PerformGetManyAsync(Guid[]? keys)
    {
        if (keys is null || keys.Length == 0)
        {
            return null;
        }

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var results = await db.RedirectUrls
                .Where(x => keys.Contains(x.Id))
                .Join(
                    db.Nodes,
                    redirect => redirect.ContentKey,
                    node => node.UniqueId,
                    (redirect, node) => new { Redirect = redirect, ContentId = node.NodeId })
                .ToListAsync();

            return results.Select(r => Map(r.Redirect, r.ContentId));
        });
    }

    /// <inheritdoc/>
    protected override async Task PersistNewItemAsync(IRedirectUrl entity) =>
        await AmbientScope.ExecuteWithContextAsync<RedirectUrlDto>(async db =>
        {
            RedirectUrlDto? dto = Map(entity);
            if (dto is null)
            {
                return;
            }

            db.RedirectUrls.Add(dto);
            await db.SaveChangesAsync();

            entity.Id = entity.Key.GetHashCode();
            entity.ResetDirtyProperties();
        });

    /// <inheritdoc/>
    protected override async Task PersistUpdatedItemAsync(IRedirectUrl entity) =>
        await AmbientScope.ExecuteWithContextAsync<RedirectUrlDto>(async db =>
        {
            RedirectUrlDto? dto = Map(entity);
            if (dto is null)
            {
                return;
            }

            await db.RedirectUrls
                .Where(x => x.Id == dto.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(x => x.ContentKey, dto.ContentKey)
                    .SetProperty(x => x.CreateDateUtc, dto.CreateDateUtc)
                    .SetProperty(x => x.Url, dto.Url)
                    .SetProperty(x => x.Culture, dto.Culture)
                    .SetProperty(x => x.UrlHash, dto.UrlHash));

            entity.ResetDirtyProperties();
        });

    private static RedirectUrlDto? Map(IRedirectUrl redirectUrl)
    {
        if (redirectUrl == null)
        {
            return null;
        }

        return new RedirectUrlDto
        {
            Id = redirectUrl.Key,
            ContentKey = redirectUrl.ContentKey,
            CreateDateUtc = redirectUrl.CreateDateUtc,
            Url = redirectUrl.Url,
            Culture = redirectUrl.Culture,
            UrlHash = redirectUrl.Url.GenerateHash<SHA1>(),
        };
    }

    private static IRedirectUrl Map(RedirectUrlDto dto, int contentId)
    {
        var url = new RedirectUrl();
        try
        {
            url.DisableChangeTracking();
            url.Key = dto.Id;
            url.Id = dto.Id.GetHashCode();
            url.ContentId = contentId;
            url.ContentKey = dto.ContentKey;
            url.CreateDateUtc = dto.CreateDateUtc;
            url.Culture = dto.Culture;
            url.Url = dto.Url;
            return url;
        }
        finally
        {
            url.EnableChangeTracking();
        }
    }
}

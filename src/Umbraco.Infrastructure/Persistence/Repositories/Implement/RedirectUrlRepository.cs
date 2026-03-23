using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class RedirectUrlRepository : EntityRepositoryBase<Guid, IRedirectUrl>, IRedirectUrlRepository
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
        IScopeAccessor scopeAccessor,
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
    public IRedirectUrl? Get(string url, Guid contentKey, string? culture)
    {
        var urlHash = url.GenerateHash<SHA1>();
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<RedirectUrlDto>(x =>
            x.Url == url && x.UrlHash == urlHash && x.ContentKey == contentKey && x.Culture == culture);
        RedirectUrlDto? dto = Database.Fetch<RedirectUrlDto>(sql).FirstOrDefault();
        return dto == null ? null : Map(dto);
    }

    /// <summary>
    /// Deletes all redirect URLs from the database.
    /// </summary>
    public void DeleteAll() => Database.Execute($"DELETE FROM {QuoteTableName("umbracoRedirectUrl")}");

    /// <summary>
    /// Deletes all redirect URLs associated with the specified content key.
    /// </summary>
    /// <param name="contentKey">The unique identifier of the content whose redirect URLs should be deleted.</param>
    public void DeleteContentUrls(Guid contentKey) =>
        Database.Execute($"DELETE FROM {QuoteTableName("umbracoRedirectUrl")} WHERE {QuoteColumnName("contentKey")}=@contentKey", new { contentKey });

    /// <summary>
    /// Deletes the redirect URL entry identified by the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the redirect URL entry to delete.</param>

    public void Delete(Guid id) => Database.Delete<RedirectUrlDto>(id);

    /// <summary>
    /// Gets the most recent redirect URL matching the specified URL.
    /// </summary>
    /// <param name="url">The URL to find the most recent redirect for.</param>
    /// <returns>The most recent <see cref="Umbraco.Cms.Core.Models.IRedirectUrl"/> if found; otherwise, null.</returns>
    public IRedirectUrl? GetMostRecentUrl(string url)
    {
        Sql<ISqlContext> sql = GetMostRecentSql(url);
        List<RedirectUrlDto> dtos = Database.Fetch<RedirectUrlDto>(sql);
        RedirectUrlDto? dto = dtos.FirstOrDefault();
        return dto == null ? null : Map(dto);
    }

    /// <summary>
    /// Asynchronously retrieves the most recent redirect URL that matches the specified URL.
    /// </summary>
    /// <param name="url">The URL for which to find the most recent redirect.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the most recent <see cref="Umbraco.Cms.Core.Models.IRedirectUrl"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<IRedirectUrl?> GetMostRecentUrlAsync(string url)
    {
        Sql<ISqlContext> sql = GetMostRecentSql(url);
        List<RedirectUrlDto> dtos = await Database.FetchAsync<RedirectUrlDto>(sql);
        RedirectUrlDto? dto = dtos.FirstOrDefault();
        return dto == null ? null : Map(dto);
    }

    private Sql<ISqlContext> GetMostRecentSql(string url)
    {
        var urlHash = url.GenerateHash<SHA1>();
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where<RedirectUrlDto>(x => x.Url == url && x.UrlHash == urlHash)
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        return sql;
    }

    public IRedirectUrl? GetMostRecentUrl(string url, string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return GetMostRecentUrl(url);
        }

        Sql<ISqlContext> sql = GetMostRecentUrlSql(url, culture);

        List<RedirectUrlDto> dtos = Database.Fetch<RedirectUrlDto>(sql);
        RedirectUrlDto? dto = dtos.FirstOrDefault(f => culture.InvariantEquals(f.Culture));

        if (dto == null)
        {
            dto = dtos.FirstOrDefault(f => string.IsNullOrWhiteSpace(f.Culture));
        }

        return dto == null ? null : Map(dto);
    }

    private Sql<ISqlContext> GetMostRecentUrlSql(string url, string culture)
    {
        var urlHash = url.GenerateHash<SHA1>();
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where<RedirectUrlDto>(x => x.Url == url && x.UrlHash == urlHash &&
                                        (x.Culture == culture.ToLower() || x.Culture == null ||
                                         x.Culture == string.Empty))
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        return sql;
    }

    public async Task<IRedirectUrl?> GetMostRecentUrlAsync(string url, string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return GetMostRecentUrl(url);
        }

        Sql<ISqlContext> sql = GetMostRecentUrlSql(url, culture);

        List<RedirectUrlDto> dtos = await Database.FetchAsync<RedirectUrlDto>(sql);
        RedirectUrlDto? dto = dtos.FirstOrDefault(f => culture.InvariantEquals(f.Culture));

        if (dto == null)
        {
            dto = dtos.FirstOrDefault(f => string.IsNullOrWhiteSpace(f.Culture));
        }

        return dto == null ? null : Map(dto);
    }

    /// <summary>
    /// Retrieves all redirect URLs that are associated with the specified content item.
    /// </summary>
    /// <param name="contentKey">The unique key (GUID) identifying the content item.</param>
    /// <returns>An enumerable collection of <see cref="IRedirectUrl"/> instances representing the redirect URLs for the specified content.</returns>
    public IEnumerable<IRedirectUrl> GetContentUrls(Guid contentKey)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where<RedirectUrlDto>(x => x.ContentKey == contentKey)
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        List<RedirectUrlDto> dtos = Database.Fetch<RedirectUrlDto>(sql);
        return dtos.Select(Map).WhereNotNull();
    }

    /// <summary>
    /// Retrieves a paged collection of redirect URLs, ordered by creation date descending.
    /// </summary>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The maximum number of redirect URLs to return in the page.</param>
    /// <param name="total">When this method returns, contains the total number of redirect URLs available across all pages.</param>
    /// <returns>
    /// An enumerable collection of <see cref="Umbraco.Cms.Core.Models.IRedirectUrl"/> instances representing the redirect URLs for the specified page.
    /// </returns>
    public IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        Page<RedirectUrlDto> result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
        total = Convert.ToInt32(result.TotalItems);
        return result.Items.Select(Map).WhereNotNull();
    }

    /// <summary>
    /// Retrieves a paged collection of all redirect URLs, ordered by creation date descending.
    /// </summary>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="total">Outputs the total number of redirect URLs available.</param>
    /// <returns>An enumerable collection of redirect URLs for the specified page.</returns
    public IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where(
                string.Format(
                    "{0}.{1} LIKE @path",
                    QuoteTableName("umbracoNode"),
                    QuoteColumnName("path")),
                new { path = "%," + rootContentId + ",%" })
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        Page<RedirectUrlDto> result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
        total = Convert.ToInt32(result.TotalItems);

        IEnumerable<IRedirectUrl> rules = result.Items.Select(Map).WhereNotNull();
        return rules;
    }

    /// <summary>
    /// Searches for redirect URLs whose URL contains the specified search term, with results paged according to the given page index and size.
    /// </summary>
    /// <param name="searchTerm">The term to search for within redirect URLs. The search is case-insensitive and matches any part of the URL.</param>
    /// <param name="pageIndex">The zero-based index of the page of results to retrieve.</param>
    /// <param name="pageSize">The number of redirect URLs to include in a single page of results.</param>
    /// <param name="total">When this method returns, contains the total number of redirect URLs matching the search term.</param>
    /// <returns>An enumerable collection of <see cref="IRedirectUrl"/> objects that match the search criteria for the specified page.</returns>
    public IEnumerable<IRedirectUrl> SearchUrls(string searchTerm, long pageIndex, int pageSize, out long total)
    {
        var wcPlaceholder = SqlSyntax.GetWildcardPlaceholder();
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .WhereLike<RedirectUrlDto>(x => x.Url, wcPlaceholder + searchTerm.Trim().ToLowerInvariant() + wcPlaceholder)
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        Page<RedirectUrlDto> result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
        total = Convert.ToInt32(result.TotalItems);

        IEnumerable<IRedirectUrl> rules = result.Items.Select(Map).WhereNotNull();
        return rules;
    }

    protected override int PerformCount(IQuery<IRedirectUrl>? query) =>
        throw new NotSupportedException("This repository does not support this method.");

    protected override bool PerformExists(Guid id) => PerformGet(id) != null;

    protected override IRedirectUrl? PerformGet(Guid id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<RedirectUrlDto>(x => x.Id == id);
        RedirectUrlDto? dto = Database.FirstOrDefault<RedirectUrlDto>(sql);
        return dto == null ? null : Map(dto);
    }

    protected override IEnumerable<IRedirectUrl> PerformGetAll(params Guid[]? ids)
    {
        if (ids?.Length > Constants.Sql.MaxParameterCount)
        {
            throw new NotSupportedException(
                $"This repository does not support more than {Constants.Sql.MaxParameterCount} ids.");
        }

        Sql<ISqlContext> sql = GetBaseQuery(false).WhereIn<RedirectUrlDto>(x => x.Id, ids);
        List<RedirectUrlDto> dtos = Database.Fetch<RedirectUrlDto>(sql);
        return dtos.WhereNotNull().Select(Map).WhereNotNull();
    }

    protected override IEnumerable<IRedirectUrl> PerformGetByQuery(IQuery<IRedirectUrl> query) =>
        throw new NotSupportedException("This repository does not support this method.");

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();
        if (isCount)
        {
            sql.Select($@"COUNT(*)
FROM {QuoteTableName("umbracoRedirectUrl")}
JOIN {QuoteTableName(NodeDto.TableName)}
ON {QuoteTableName("umbracoRedirectUrl")}.{QuoteColumnName("contentKey")}={QuoteTableName(NodeDto.TableName)}.{QuoteColumnName("uniqueId")}");
        }
        else
        {
            sql.Select($@"{QuoteTableName("umbracoRedirectUrl")}.*, {QuoteTableName(NodeDto.TableName)}.id AS contentId
FROM {QuoteTableName("umbracoRedirectUrl")}
JOIN {QuoteTableName(NodeDto.TableName)}
ON {QuoteTableName("umbracoRedirectUrl")}.{QuoteColumnName("contentKey")}={QuoteTableName(NodeDto.TableName)}.{QuoteColumnName("uniqueId")}");
        }

        return sql;
    }

    protected override string GetBaseWhereClause() => "id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string> { $"DELETE FROM {QuoteTableName("umbracoRedirectUrl")} WHERE id = @id" };
        return list;
    }

    protected override void PersistNewItem(IRedirectUrl entity)
    {
        RedirectUrlDto? dto = Map(entity);
        Database.Insert(dto);
        entity.Id = entity.Key.GetHashCode();
    }

    protected override void PersistUpdatedItem(IRedirectUrl entity)
    {
        RedirectUrlDto? dto = Map(entity);
        if (dto is not null)
        {
            Database.Update(dto);
        }
    }

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

    private static IRedirectUrl? Map(RedirectUrlDto dto)
    {
        if (dto == null)
        {
            return null;
        }

        var url = new RedirectUrl();
        try
        {
            url.DisableChangeTracking();
            url.Key = dto.Id;
            url.Id = dto.Id.GetHashCode();
            url.ContentId = dto.ContentId;
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

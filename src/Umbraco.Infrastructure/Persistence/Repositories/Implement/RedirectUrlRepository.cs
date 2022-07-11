using System.Security.Cryptography;
using System.Threading.Tasks;
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

internal class RedirectUrlRepository : EntityRepositoryBase<Guid, IRedirectUrl>, IRedirectUrlRepository
{
    public RedirectUrlRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<RedirectUrlRepository> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    public IRedirectUrl? Get(string url, Guid contentKey, string? culture)
    {
        var urlHash = url.GenerateHash<SHA1>();
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<RedirectUrlDto>(x =>
            x.Url == url && x.UrlHash == urlHash && x.ContentKey == contentKey && x.Culture == culture);
        RedirectUrlDto? dto = Database.Fetch<RedirectUrlDto>(sql).FirstOrDefault();
        return dto == null ? null : Map(dto);
    }

    public void DeleteAll() => Database.Execute("DELETE FROM umbracoRedirectUrl");

    public void DeleteContentUrls(Guid contentKey) =>
        Database.Execute("DELETE FROM umbracoRedirectUrl WHERE contentKey=@contentKey", new { contentKey });

    public void Delete(Guid id) => Database.Delete<RedirectUrlDto>(id);

    public IRedirectUrl? GetMostRecentUrl(string url)
    {
        Sql<ISqlContext> sql = GetMostRecentSql(url);
        List<RedirectUrlDto> dtos = Database.Fetch<RedirectUrlDto>(sql);
        RedirectUrlDto? dto = dtos.FirstOrDefault();
        return dto == null ? null : Map(dto);
    }

    public  async Task<IRedirectUrl?> GetMostRecentUrlAsync(string url)
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
        RedirectUrlDto? dto = dtos.FirstOrDefault(f => f.Culture == culture.ToLower());

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
        RedirectUrlDto? dto = dtos.FirstOrDefault(f => f.Culture == culture.ToLower());

        if (dto == null)
        {
            dto = dtos.FirstOrDefault(f => string.IsNullOrWhiteSpace(f.Culture));
        }

        return dto == null ? null : Map(dto);
    }

    public IEnumerable<IRedirectUrl> GetContentUrls(Guid contentKey)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where<RedirectUrlDto>(x => x.ContentKey == contentKey)
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        List<RedirectUrlDto> dtos = Database.Fetch<RedirectUrlDto>(sql);
        return dtos.Select(Map).WhereNotNull();
    }

    public IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        Page<RedirectUrlDto> result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
        total = Convert.ToInt32(result.TotalItems);
        return result.Items.Select(Map).WhereNotNull();
    }

    public IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where(
                string.Format("{0}.{1} LIKE @path", SqlSyntax.GetQuotedTableName("umbracoNode"),
                    SqlSyntax.GetQuotedColumnName("path")), new { path = "%," + rootContentId + ",%" })
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        Page<RedirectUrlDto> result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
        total = Convert.ToInt32(result.TotalItems);

        IEnumerable<IRedirectUrl> rules = result.Items.Select(Map).WhereNotNull();
        return rules;
    }

    public IEnumerable<IRedirectUrl> SearchUrls(string searchTerm, long pageIndex, int pageSize, out long total)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false)
            .Where(
                string.Format("{0}.{1} LIKE @url", SqlSyntax.GetQuotedTableName("umbracoRedirectUrl"),
                    SqlSyntax.GetQuotedColumnName("Url")),
                new { url = "%" + searchTerm.Trim().ToLowerInvariant() + "%" })
            .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
        Page<RedirectUrlDto> result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
        total = Convert.ToInt32(result.TotalItems);

        IEnumerable<IRedirectUrl> rules = result.Items.Select(Map).WhereNotNull();
        return rules;
    }

    protected override int PerformCount(IQuery<IRedirectUrl> query) =>
        throw new NotSupportedException("This repository does not support this method.");

    protected override bool PerformExists(Guid id) => PerformGet(id) != null;

    protected override IRedirectUrl? PerformGet(Guid id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<RedirectUrlDto>(x => x.Id == id);
        RedirectUrlDto? dto = Database.Fetch<RedirectUrlDto>(sql.SelectTop(1)).FirstOrDefault();
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
            sql.Select(@"COUNT(*)
FROM umbracoRedirectUrl
JOIN umbracoNode ON umbracoRedirectUrl.contentKey=umbracoNode.uniqueID");
        }
        else
        {
            sql.Select(@"umbracoRedirectUrl.*, umbracoNode.id AS contentId
FROM umbracoRedirectUrl
JOIN umbracoNode ON umbracoRedirectUrl.contentKey=umbracoNode.uniqueID");
        }

        return sql;
    }

    protected override string GetBaseWhereClause() => "id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string> { "DELETE FROM umbracoRedirectUrl WHERE id = @id" };
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

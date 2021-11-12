using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class RedirectUrlRepository : NPocoRepositoryBase<Guid, IRedirectUrl>, IRedirectUrlRepository
    {
        public RedirectUrlRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override int PerformCount(IQuery<IRedirectUrl> query)
        {
            throw new NotSupportedException("This repository does not support this method.");
        }

        protected override bool PerformExists(Guid id)
        {
            return PerformGet(id) != null;
        }

        protected override IRedirectUrl PerformGet(Guid id)
        {
            var sql = GetBaseQuery(false).Where<RedirectUrlDto>(x => x.Id == id);
            var dto = Database.Fetch<RedirectUrlDto>(sql.SelectTop(1)).FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        protected override IEnumerable<IRedirectUrl> PerformGetAll(params Guid[] ids)
        {
            if (ids.Length > Constants.Sql.MaxParameterCount)
                throw new NotSupportedException($"This repository does not support more than {Constants.Sql.MaxParameterCount} ids.");

            var sql = GetBaseQuery(false).WhereIn<RedirectUrlDto>(x => x.Id, ids);
            var dtos = Database.Fetch<RedirectUrlDto>(sql);
            return dtos.WhereNotNull().Select(Map);
        }

        protected override IEnumerable<IRedirectUrl> PerformGetByQuery(IQuery<IRedirectUrl> query)
        {
            throw new NotSupportedException("This repository does not support this method.");
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();
            if (isCount)
                sql.Select(@"COUNT(*)
FROM umbracoRedirectUrl
JOIN umbracoNode ON umbracoRedirectUrl.contentKey=umbracoNode.uniqueID");
            else
                sql.Select(@"umbracoRedirectUrl.*, umbracoNode.id AS contentId
FROM umbracoRedirectUrl
JOIN umbracoNode ON umbracoRedirectUrl.contentKey=umbracoNode.uniqueID");
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM umbracoRedirectUrl WHERE id = @id"
                };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(IRedirectUrl entity)
        {
            var dto = Map(entity);
            Database.Insert(dto);
            entity.Id = entity.Key.GetHashCode();
        }

        protected override void PersistUpdatedItem(IRedirectUrl entity)
        {
            var dto = Map(entity);
            Database.Update(dto);
        }

        private static RedirectUrlDto Map(IRedirectUrl redirectUrl)
        {
            if (redirectUrl == null) return null;

            return new RedirectUrlDto
            {
                Id = redirectUrl.Key,
                ContentKey = redirectUrl.ContentKey,
                CreateDateUtc = redirectUrl.CreateDateUtc,
                Url = redirectUrl.Url,
                Culture = redirectUrl.Culture,
                UrlHash = redirectUrl.Url.GenerateHash<SHA1>()
            };
        }

        private static IRedirectUrl Map(RedirectUrlDto dto)
        {
            if (dto == null) return null;

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

        public IRedirectUrl Get(string url, Guid contentKey, string culture)
        {
            var urlHash = url.GenerateHash<SHA1>();
            var sql = GetBaseQuery(false).Where<RedirectUrlDto>(x => x.Url == url && x.UrlHash == urlHash && x.ContentKey == contentKey && x.Culture == culture);
            var dto = Database.Fetch<RedirectUrlDto>(sql).FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        public void DeleteAll()
        {
            Database.Execute("DELETE FROM umbracoRedirectUrl");
        }

        public void DeleteContentUrls(Guid contentKey)
        {
            Database.Execute("DELETE FROM umbracoRedirectUrl WHERE contentKey=@contentKey", new { contentKey });
        }

        public void Delete(Guid id)
        {
            Database.Delete<RedirectUrlDto>(id);
        }

        public IRedirectUrl GetMostRecentUrl(string url)
        {
            var urlHash = url.GenerateHash<SHA1>();
            var sql = GetBaseQuery(false)
                .Where<RedirectUrlDto>(x => x.Url == url && x.UrlHash == urlHash)
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
            var dtos = Database.Fetch<RedirectUrlDto>(sql);
            var dto = dtos.FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        public IRedirectUrl GetMostRecentUrl(string url, string culture)
        {
            if (string.IsNullOrWhiteSpace(culture)) return GetMostRecentUrl(url);
            var urlHash = url.GenerateHash<SHA1>();
            var sql = GetBaseQuery(false)
                .Where<RedirectUrlDto>(x => x.Url == url && x.UrlHash == urlHash &&
                    (x.Culture == culture.ToLower() || x.Culture == string.Empty))
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
            var dtos = Database.Fetch<RedirectUrlDto>(sql);
            var dto = dtos.FirstOrDefault(f => f.Culture == culture.ToLower());

            if (dto == null)
                dto = dtos.FirstOrDefault(f => f.Culture == string.Empty);

            return dto == null ? null : Map(dto);
        }

        public IEnumerable<IRedirectUrl> GetContentUrls(Guid contentKey)
        {
            var sql = GetBaseQuery(false)
                .Where<RedirectUrlDto>(x => x.ContentKey == contentKey)
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
            var dtos = Database.Fetch<RedirectUrlDto>(sql);
            return dtos.Select(Map);
        }

        public IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total)
        {
            var sql = GetBaseQuery(false)
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
            var result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
            total = Convert.ToInt32(result.TotalItems);
            return result.Items.Select(Map);
        }

        public IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total)
        {
            var sql = GetBaseQuery(false)
                .Where(string.Format("{0}.{1} LIKE @path", SqlSyntax.GetQuotedTableName("umbracoNode"), SqlSyntax.GetQuotedColumnName("path")), new { path = "%," + rootContentId + ",%" })
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
            var result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
            total = Convert.ToInt32(result.TotalItems);

            var rules = result.Items.Select(Map);
            return rules;
        }

        public IEnumerable<IRedirectUrl> SearchUrls(string searchTerm, long pageIndex, int pageSize, out long total)
        {
            var sql = GetBaseQuery(false)
                .Where(string.Format("{0}.{1} LIKE @url", SqlSyntax.GetQuotedTableName("umbracoRedirectUrl"), SqlSyntax.GetQuotedColumnName("Url")), new { url = "%" + searchTerm.Trim().ToLowerInvariant() + "%" })
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc);
            var result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
            total = Convert.ToInt32(result.TotalItems);

            var rules = result.Items.Select(Map);
            return rules;
        }
    }
}

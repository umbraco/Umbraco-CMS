using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class RedirectUrlRepository : PetaPocoRepositoryBase<int, IRedirectUrl>, IRedirectUrlRepository
    {
        public RedirectUrlRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax) 
            : base(work, cache, logger, sqlSyntax)
        { }

        protected override int PerformCount(IQuery<IRedirectUrl> query)
        {
            throw new NotSupportedException("This repository does not support this method.");
        }

        protected override bool PerformExists(int id)
        {
            return PerformGet(id) != null;
        }

        protected override IRedirectUrl PerformGet(int id)
        {
            var sql = GetBaseQuery(false).Where<RedirectUrlDto>(x => x.Id == id);
            var dto = Database.Fetch<RedirectUrlDto>(sql).FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        protected override IEnumerable<IRedirectUrl> PerformGetAll(params int[] ids)
        {
            if (ids.Length > 2000)
                throw new NotSupportedException("This repository does not support more than 2000 ids.");
            var sql = GetBaseQuery(false).WhereIn<RedirectUrlDto>(x => x.Id, ids);
            var dtos = Database.Fetch<RedirectUrlDto>(sql);
            return dtos.WhereNotNull().Select(Map);
        }

        protected override IEnumerable<IRedirectUrl> PerformGetByQuery(IQuery<IRedirectUrl> query)
        {
            throw new NotSupportedException("This repository does not support this method.");
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
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
            return "id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    "DELETE FROM umbracoRedirectUrl WHERE id = @Id"
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
            entity.Id = dto.Id;
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
                Id = redirectUrl.Id,
                ContentKey = redirectUrl.ContentKey,
                CreateDateUtc = redirectUrl.CreateDateUtc,
                Url = redirectUrl.Url,
                Hurl = HashUrl(redirectUrl.Url)
            };
        }

        private static IRedirectUrl Map(RedirectUrlDto dto)
        {
            if (dto == null) return null;

            var url = new RedirectUrl();
            try
            {
                url.DisableChangeTracking();
                url.Id = dto.Id;
                url.ContentId = dto.ContentId;
                url.ContentKey = dto.ContentKey;
                url.CreateDateUtc = dto.CreateDateUtc;
                url.Url = dto.Url;
                return url;
            }
            finally
            {
                url.EnableChangeTracking();
            }
        }

        public IRedirectUrl Get(string url, Guid contentKey)
        {
            var hurl = HashUrl(url);
            var sql = GetBaseQuery(false).Where<RedirectUrlDto>(x => x.Url == url && x.Hurl == hurl && x.ContentKey == contentKey);
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

        public void Delete(int id)
        {
            Database.Delete<RedirectUrlDto>(id);
        }

        public IRedirectUrl GetMostRecentUrl(string url)
        {
            var hurl = HashUrl(url);
            var sql = GetBaseQuery(false)
                .Where<RedirectUrlDto>(x => x.Url == url && x.Hurl == hurl)
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc, SqlSyntax);
            var dtos = Database.Fetch<RedirectUrlDto>(sql);
            var dto = dtos.FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        public IEnumerable<IRedirectUrl> GetContentUrls(Guid contentKey)
        {
            var sql = GetBaseQuery(false)
                .Where<RedirectUrlDto>(x => x.ContentKey == contentKey)
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc, SqlSyntax);
            var dtos = Database.Fetch<RedirectUrlDto>(sql);
            return dtos.Select(Map);
        }

        public IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total)
        {
            var sql = GetBaseQuery(false)
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc, SqlSyntax);
            var result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
            total = Convert.ToInt32(result.TotalItems);
            return result.Items.Select(Map);
        }

        public IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total)
        {
            var sql = GetBaseQuery(false)
                .Where("umbracoNode.path LIKE @path", new { path = "%," + rootContentId + ",%" })
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc, SqlSyntax);
            var result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
            total = Convert.ToInt32(result.TotalItems);

            var rules = result.Items.Select(Map);
            return rules;
        }

        private static string HashUrl(string url)
        {
            var h = new MD5CryptoServiceProvider();
            var i = Encoding.UTF8.GetBytes(url);
            var o = h.ComputeHash(i);
            return Encoding.UTF8.GetString(o);
        }
    }
}

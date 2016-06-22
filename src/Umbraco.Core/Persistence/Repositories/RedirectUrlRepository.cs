using System;
using System.Collections.Generic;
using System.Linq;
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
            sql.Select(isCount ? "COUNT(*)" : "*").From<RedirectUrlDto>(SqlSyntax);
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
                ContentId = redirectUrl.ContentId,
                CreateDateUtc = redirectUrl.CreateDateUtc,
                Url = redirectUrl.Url
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
                url.CreateDateUtc = dto.CreateDateUtc;
                url.Url = dto.Url;
                return url;
            }
            finally
            {
                url.EnableChangeTracking();
            }
        }

        public IRedirectUrl Get(string url, int contentId)
        {
            var sql = GetBaseQuery(false).Where<RedirectUrlDto>(x => x.Url == url && x.ContentId == contentId);
            var dto = Database.Fetch<RedirectUrlDto>(sql).FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        public void DeleteAll()
        {
            Database.Execute("DELETE FROM umbracoRedirectUrl");
        }

        public void DeleteContentUrls(int contentId)
        {
            Database.Execute("DELETE FROM umbracoRedirectUrl WHERE contentId=@contentId", new { contentId });
        }

        public void Delete(int id)
        {
            Database.Delete<RedirectUrlDto>(id);
        }

        public IRedirectUrl GetMostRecentUrl(string url)
        {
            var dtos = Database.Fetch<RedirectUrlDto>("SELECT * FROM umbracoRedirectUrl WHERE url=@url ORDER BY createDateUtc DESC;",
                new { url });
            var dto = dtos.FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        public IEnumerable<IRedirectUrl> GetContentUrls(int contentId)
        {
            var dtos = Database.Fetch<RedirectUrlDto>("SELECT * FROM umbracoRedirectUrl WHERE contentId=@id ORDER BY createDateUtc DESC;",
                new { id = contentId });
            return dtos.Select(Map);
        }

        public IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total)
        {
            var sql = GetBaseQuery(false).OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc, SqlSyntax);
            var result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
            total = Convert.ToInt32(result.TotalItems);
            return result.Items.Select(Map);
        }

        public IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total)
        {
            var sql = GetBaseQuery(false)
                .InnerJoin<NodeDto>(SqlSyntax).On<NodeDto, RedirectUrlDto>(SqlSyntax, left => left.NodeId, right => right.ContentId)
                .Where("umbracoNode.path LIKE @path", new { path = "%," + rootContentId + ",%" })
                .OrderByDescending<RedirectUrlDto>(x => x.CreateDateUtc, SqlSyntax);
            var result = Database.Page<RedirectUrlDto>(pageIndex + 1, pageSize, sql);
            total = Convert.ToInt32(result.TotalItems);

            var rules = result.Items.Select(Map);
            return rules;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    class RedirectUrlService : RepositoryService, IRedirectUrlService
    {
        public RedirectUrlService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory) 
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        { }

        public void Save(RedirectUrl redirectUrl)
        {
            // check if the url already exists
            // the url actually is a primary key?
            // though we might want to keep the history?

            using (var uow = UowProvider.GetUnitOfWork())
            {
                var dto = new RedirectUrlDto
                {
                    Id = redirectUrl.Id,
                    ContentId = redirectUrl.ContentId,
                    CreateDateUtc = redirectUrl.CreateDateUtc,
                    Url = redirectUrl.Url
                };
                uow.Database.InsertOrUpdate(dto);
                uow.Commit();
                redirectUrl.Id = dto.Id;
            }
        }

        public void Delete(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                uow.Database.Execute("DELETE FROM umbracoContentUrlRule WHERE id=@id", new { id = id });
                uow.Commit();
            }
        }

        public void DeleteContentRules(int contentId)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                uow.Database.Execute("DELETE FROM umbracoContentUrlRule WHERE contenId=@id", new { id = contentId });
                uow.Commit();
            }
        }

        public void DeleteAll()
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                uow.Database.Execute("DELETE FROM umbracoContentUrlRule;");
                uow.Commit();
            }
        }

        public RedirectUrl GetMostRecentRule(string url)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var ruleDtos = uow.Database.Fetch<RedirectUrlDto>("SELECT * FROM umbracoContentUrlRule WHERE url=@url ORDER BY createDateUtc DESC;",
                    new { url });
                var ruleDto = ruleDtos.FirstOrDefault();
                var rule = ruleDto == null ? null : new RedirectUrl
                {
                    Id = ruleDto.Id,
                    ContentId = ruleDto.ContentId,
                    CreateDateUtc = ruleDto.CreateDateUtc,
                    Url = ruleDto.Url
                };
                uow.Commit();
                return rule;
            }
        }

        public IEnumerable<RedirectUrl> GetRules(int contentId)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var ruleDtos = uow.Database.Fetch<RedirectUrlDto>("SELECT * FROM umbracoContentUrlRule WHERE contentId=@id ORDER BY createDateUtc DESC;",
                    new { id = contentId });
                var rules = ruleDtos.Select(x=> new RedirectUrl
                {
                    Id = x.Id,
                    ContentId = x.ContentId,
                    CreateDateUtc = x.CreateDateUtc,
                    Url = x.Url
                });
                uow.Commit();
                return rules;
            }
        }

        public IEnumerable<RedirectUrl> GetAllRules(long pageIndex, int pageSize, out long total)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var ruleDtos = uow.Database.Fetch<RedirectUrlDto>("SELECT * FROM umbracoContentUrlRule ORDER BY createDateUtc DESC;");
                var rules = ruleDtos.Select(x => new RedirectUrl
                {
                    Id = x.Id,
                    ContentId = x.ContentId,
                    CreateDateUtc = x.CreateDateUtc,
                    Url = x.Url
                }).ToArray();
                total = rules.Length;
                uow.Commit();
                return rules;
            }
        }

        public IEnumerable<RedirectUrl> GetAllRules(int rootContentId, long pageIndex, int pageSize, out long total)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var path = "%," + rootContentId + ",%";

                var ruleDtos = uow.Database.Fetch<RedirectUrlDto>(@"SELECT * FROM umbracoContentUrlRule
JOIN umbracoNode ON umbracoNode.id=umbracoContentUrlRule.contentId
WHERE umbracoNode.path LIKE @path
ORDER BY createDateUtc DESC;", new { path });
                var rules = ruleDtos.Select(x => new RedirectUrl
                {
                    Id = x.Id,
                    ContentId = x.ContentId,
                    CreateDateUtc = x.CreateDateUtc,
                    Url = x.Url
                }).ToArray();
                total = rules.Length;
                uow.Commit();
                return rules;
            }
        }
    }
}

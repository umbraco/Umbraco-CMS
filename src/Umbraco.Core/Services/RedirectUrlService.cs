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

        public void Save(IRedirectUrl redirectUrl)
        {
            // check if the url already exists
            // the url actually is a primary key?
            // though we might want to keep the history?

            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                repo.AddOrUpdate(redirectUrl);
                uow.Commit();
            }
        }

        public void Delete(IRedirectUrl redirectUrl)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                repo.Delete(redirectUrl);
                uow.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                repo.Delete(id);
                uow.Commit();
            }
        }

        public void DeleteContentUrls(int contentId)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                repo.DeleteContentUrls(contentId);
                uow.Commit();
            }
        }

        public void DeleteAll()
        {
            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                repo.DeleteAll();
                uow.Commit();
            }
        }

        public IRedirectUrl GetMostRecentRule(string url)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                var rule = repo.GetMostRecentRule(url);
                uow.Commit();
                return rule;
            }
        }

        public IEnumerable<IRedirectUrl> GetContentUrls(int contentId)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                var rules = repo.GetContentUrls(contentId);
                uow.Commit();
                return rules;
            }
        }

        public IEnumerable<IRedirectUrl> GetAllUrls(long pageIndex, int pageSize, out long total)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                var rules = repo.GetAllUrls(pageIndex, pageSize, out total);
                uow.Commit();
                return rules;
            }
        }

        public IEnumerable<IRedirectUrl> GetAllUrls(int rootContentId, long pageIndex, int pageSize, out long total)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            using (var repo = RepositoryFactory.CreateRedirectUrlRepository(uow))
            {
                var rules = repo.GetAllUrls(rootContentId, pageIndex, pageSize, out total);
                uow.Commit();
                return rules;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal class RedirectUrlService : RepositoryService, IRedirectUrlService
    {
        public RedirectUrlService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory) 
            : base(provider, logger, eventMessagesFactory)
        { }

        public void Register(string url, Guid contentKey)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                var redir = repo.Get(url, contentKey);
                if (redir != null)
                    redir.CreateDateUtc = DateTime.UtcNow;
                else
                    redir = new RedirectUrl { Key = Guid.NewGuid(), Url = url, ContentKey = contentKey };
                repo.AddOrUpdate(redir);
                uow.Complete();
            }
        }
        
        public void Delete(IRedirectUrl redirectUrl)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                repo.Delete(redirectUrl);
                uow.Complete();
            }
        }

        public void Delete(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                repo.Delete(id);
                uow.Complete();
            }
        }

        public void DeleteContentRedirectUrls(Guid contentKey)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                repo.DeleteContentUrls(contentKey);
                uow.Complete();
            }
        }

        public void DeleteAll()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                repo.DeleteAll();
                uow.Complete();
            }
        }

        public IRedirectUrl GetMostRecentRedirectUrl(string url)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                var rule = repo.GetMostRecentUrl(url);
                uow.Complete();
                return rule;
            }
        }

        public IEnumerable<IRedirectUrl> GetContentRedirectUrls(Guid contentKey)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                var rules = repo.GetContentUrls(contentKey);
                uow.Complete();
                return rules;
            }
        }

        public IEnumerable<IRedirectUrl> GetAllRedirectUrls(long pageIndex, int pageSize, out long total)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                var rules = repo.GetAllUrls(pageIndex, pageSize, out total);
                uow.Complete();
                return rules;
            }
        }

        public IEnumerable<IRedirectUrl> GetAllRedirectUrls(int rootContentId, long pageIndex, int pageSize, out long total)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                var rules = repo.GetAllUrls(rootContentId, pageIndex, pageSize, out total);
                uow.Complete();
                return rules;
            }
        }
        public IEnumerable<IRedirectUrl> SearchRedirectUrls(string searchTerm, long pageIndex, int pageSize, out long total)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IRedirectUrlRepository>();
                var rules = repo.SearchUrls(searchTerm, pageIndex, pageSize, out total);
                uow.Complete();
                return rules;
            }
        }
    }
}

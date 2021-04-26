﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Implement
{
    internal class RedirectUrlService : RepositoryService, IRedirectUrlService
    {
        private readonly IRedirectUrlRepository _redirectUrlRepository;

        public RedirectUrlService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory,
            IRedirectUrlRepository redirectUrlRepository)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _redirectUrlRepository = redirectUrlRepository;
        }

        public void Register(string url, Guid contentKey, string culture = null)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var redir = _redirectUrlRepository.Get(url, contentKey, culture);
                if (redir != null)
                    redir.CreateDateUtc = DateTime.UtcNow;
                else
                    redir = new RedirectUrl { Key = Guid.NewGuid(), Url = url, ContentKey = contentKey, Culture = culture};
                _redirectUrlRepository.Save(redir);
                scope.Complete();
            }
        }

        public void Delete(IRedirectUrl redirectUrl)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _redirectUrlRepository.Delete(redirectUrl);
                scope.Complete();
            }
        }

        public void Delete(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _redirectUrlRepository.Delete(id);
                scope.Complete();
            }
        }

        public void DeleteContentRedirectUrls(Guid contentKey)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _redirectUrlRepository.DeleteContentUrls(contentKey);
                scope.Complete();
            }
        }

        public void DeleteAll()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _redirectUrlRepository.DeleteAll();
                scope.Complete();
            }
        }

        public IRedirectUrl GetMostRecentRedirectUrl(string url)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _redirectUrlRepository.GetMostRecentUrl(url);
            }
        }

        public IEnumerable<IRedirectUrl> GetContentRedirectUrls(Guid contentKey)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _redirectUrlRepository.GetContentUrls(contentKey);
            }
        }

        public IEnumerable<IRedirectUrl> GetAllRedirectUrls(long pageIndex, int pageSize, out long total)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _redirectUrlRepository.GetAllUrls(pageIndex, pageSize, out total);
            }
        }

        public IEnumerable<IRedirectUrl> GetAllRedirectUrls(int rootContentId, long pageIndex, int pageSize, out long total)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _redirectUrlRepository.GetAllUrls(rootContentId, pageIndex, pageSize, out total);
            }
        }

        public IEnumerable<IRedirectUrl> SearchRedirectUrls(string searchTerm, long pageIndex, int pageSize, out long total)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _redirectUrlRepository.SearchUrls(searchTerm, pageIndex, pageSize, out total);
            }
        }

        public IRedirectUrl GetMostRecentRedirectUrl(string url, string culture)
        {
            if (string.IsNullOrWhiteSpace(culture)) return GetMostRecentRedirectUrl(url);
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _redirectUrlRepository.GetMostRecentUrl(url, culture);
            }

        }
    }
}

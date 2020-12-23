using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.Infrastructure.PublishedCache.Persistence
{
    public class NuCacheContentService : RepositoryService, INuCacheContentService
    {
        private readonly INuCacheContentRepository _repository;

        public NuCacheContentService(INuCacheContentRepository repository, IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public IEnumerable<ContentNodeKit> GetAllContentSources()
            => _repository.GetAllContentSources();

        /// <inheritdoc/>
        public IEnumerable<ContentNodeKit> GetAllMediaSources()
            => _repository.GetAllMediaSources();

        /// <inheritdoc/>
        public IEnumerable<ContentNodeKit> GetBranchContentSources(int id)
            => _repository.GetBranchContentSources(id);

        /// <inheritdoc/>
        public IEnumerable<ContentNodeKit> GetBranchMediaSources(int id)
            => _repository.GetBranchMediaSources(id);

        /// <inheritdoc/>
        public ContentNodeKit GetContentSource(int id)
            => _repository.GetContentSource(id);

        /// <inheritdoc/>
        public ContentNodeKit GetMediaSource(int id)
            => _repository.GetMediaSource(id);

        /// <inheritdoc/>
        public IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int> ids)
            => _repository.GetTypeContentSources(ids);

        /// <inheritdoc/>
        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids)
            => _repository.GetTypeContentSources(ids);

        /// <inheritdoc/>
        public void DeleteContentItem(IContentBase item)
            => _repository.DeleteContentItem(item);

        /// <inheritdoc/>
        public void RefreshContent(IContent content)
            => _repository.RefreshContent(content);

        /// <inheritdoc/>
        public void RefreshEntity(IContentBase content)
            => _repository.RefreshEntity(content);

        /// <inheritdoc/>
        public void Rebuild(
            int groupSize = 5000,
            IReadOnlyCollection<int> contentTypeIds = null,
            IReadOnlyCollection<int> mediaTypeIds = null,
            IReadOnlyCollection<int> memberTypeIds = null)
        {
            using (IScope scope = ScopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                if (contentTypeIds != null)
                {
                    scope.ReadLock(Constants.Locks.ContentTree);
                }

                if (mediaTypeIds != null)
                {
                    scope.ReadLock(Constants.Locks.MediaTree);
                }

                if (memberTypeIds != null)
                {
                    scope.ReadLock(Constants.Locks.MemberTree);
                }

                _repository.Rebuild(groupSize, contentTypeIds, mediaTypeIds, memberTypeIds);
                scope.Complete();
            }
        }

        /// <inheritdoc/>
        public bool VerifyContentDbCache()
        {
            using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.ContentTree);
            return _repository.VerifyContentDbCache();
        }

        /// <inheritdoc/>
        public bool VerifyMediaDbCache()
        {
            using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MediaTree);
            return _repository.VerifyMediaDbCache();
        }

        /// <inheritdoc/>
        public bool VerifyMemberDbCache()
        {
            using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
            scope.ReadLock(Constants.Locks.MemberTree);
            return _repository.VerifyMemberDbCache();
        }
    }
}

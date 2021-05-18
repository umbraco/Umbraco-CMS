using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search
{
    public sealed class MediaIndexingNotificationHandler : INotificationHandler<MediaCacheRefresherNotification>
    {
        private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
        private readonly IMediaService _mediaService;

        public MediaIndexingNotificationHandler(IUmbracoIndexingHandler umbracoIndexingHandler, IMediaService mediaService)
        {
            _umbracoIndexingHandler = umbracoIndexingHandler ?? throw new ArgumentNullException(nameof(umbracoIndexingHandler));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        public void Handle(MediaCacheRefresherNotification args)
        {
            if (!_umbracoIndexingHandler.Enabled)
            {
                return;
            }

            if (Suspendable.ExamineEvents.CanIndex == false)
            {
                return;
            }

            if (args.MessageType != MessageType.RefreshByPayload)
            {
                throw new NotSupportedException();
            }

            foreach (var payload in (MediaCacheRefresher.JsonPayload[])args.MessageObject)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    // remove from *all* indexes
                    _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, false);
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
                    var media = _mediaService.GetById(payload.Id);
                    if (media == null)
                    {
                        // gone fishing, remove entirely
                        _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    if (media.Trashed)
                    {
                        _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, true);
                    }

                    // just that media
                    _umbracoIndexingHandler.ReIndexForMedia(media, !media.Trashed);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        const int pageSize = 500;
                        var page = 0;
                        var total = long.MaxValue;
                        while (page * pageSize < total)
                        {
                            var descendants = _mediaService.GetPagedDescendants(media.Id, page++, pageSize, out total);
                            foreach (var descendant in descendants)
                            {
                                _umbracoIndexingHandler.ReIndexForMedia(descendant, !descendant.Trashed);
                            }
                        }
                    }
                }
            }
        }
    }
}

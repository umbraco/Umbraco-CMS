using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.Redirects
{
    //when content is renamed or moved, we want to create a permanent 301 redirect from it's old url
    public class RedirectTrackingEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarting
              (UmbracoApplicationBase umbracoApplication,
               ApplicationContext applicationContext)
        {
            //create a redirect if the item is published
            // on the publishing event the previous Url won't have been updated in the cache yet
            ContentService.Publishing += ContentService_Publishing;

            // create a redirect if the item is being moved
            ContentService.Moving += ContentService_Moving;

            //rolled back items have to be published, so publishing will take care of that

            // do we want to do anything if a content item is unpublished or deleted ?
            // eg. remove any redirects to the node or associated redirects created by previous nodes
            // for now it will just 404 which is correct I think
        }
        private void ContentService_Publishing(Core.Publishing.IPublishingStrategy sender, Core.Events.PublishEventArgs<IContent> e)
        {
            foreach (var publishedEntity in e.PublishedEntities)
            {
                CreateRedirectsIfUrlHasChanged(publishedEntity);
            }
        }
        private void ContentService_Moving(IContentService sender, Core.Events.MoveEventArgs<IContent> e)
        {
            var umbracoHelper = new Umbraco.Web.UmbracoHelper(Umbraco.Web.UmbracoContext.Current);
            var redirectUrlService = ApplicationContext.Current.Services.RedirectUrlService;

            foreach (var moveEventInfo in e.MoveInfoCollection)
            {
                var entityBeingMoved = moveEventInfo.Entity;
                //entity hasn't moved yet so current Url is ?
                var currentUrl = umbracoHelper.Url(moveEventInfo.Entity.Id);
                if (!String.IsNullOrWhiteSpace(currentUrl) && currentUrl != "/")
                {
                    //create redirectUrl
                    var redirectUrl = new RedirectUrl()
                    {
                        ContentId = entityBeingMoved.Id,
                        Url = currentUrl,
                        CreateDateUtc = DateTime.UtcNow
                    };
                    redirectUrlService.Save(redirectUrl);

                    // has the moved item got descendants ?
                    CreateRedirectsForDescendants(entityBeingMoved);
                }

                // thoughts
                // is the entity being moved from the recycle bin ?
            }
        }
        private void CreateRedirectsIfUrlHasChanged(IContent entity)
        {
            var umbracoHelper = new Umbraco.Web.UmbracoHelper(Umbraco.Web.UmbracoContext.Current);
            var redirectUrlService = ApplicationContext.Current.Services.RedirectUrlService;
            //url won't have changed in the cache yet
            var currentUrl = umbracoHelper.UrlAbsolute(entity.Id);
            if (!String.IsNullOrWhiteSpace(currentUrl))
            {
                //get last segment of current url    
                var currentUri = new Uri(currentUrl);
                var currentLastSegment = (currentUri != null && currentUri.Segments.Any()) ? currentUri.Segments.LastOrDefault().Replace("/", "") : String.Empty;
                // get segment of update entity
                var updatedUrlSegment = entity.GetUrlSegment();
                if (!String.IsNullOrWhiteSpace(currentLastSegment) && !currentLastSegment.InvariantEquals(updatedUrlSegment))
                {
                    //url has changed...
                    //create redirectUrl
                    var redirectUrl = new RedirectUrl()
                    {
                        ContentId = entity.Id,
                        Url = currentUrl,
                        CreateDateUtc = DateTime.UtcNow
                    };
                    //create redirect for any descendants
                    CreateRedirectsForDescendants(entity);
                }
            }
        }
        private void CreateRedirectsForDescendants(IContent entity)
        {
            var umbracoHelper = new Umbraco.Web.UmbracoHelper(Umbraco.Web.UmbracoContext.Current);
            var redirectUrlService = ApplicationContext.Current.Services.RedirectUrlService;
            var descendants = entity.Descendants();
            if (descendants.Any())
            {
                foreach (var descendant in descendants)
                {
                    // create redirect url for each descendant item
                    var currentUrl = umbracoHelper.Url(descendant.Id);
                    if (!String.IsNullOrWhiteSpace(currentUrl))
                    {
                        //create redirectUrl
                        var redirectUrl = new RedirectUrl()
                        {
                            ContentId = descendant.Id,
                            Url = currentUrl,
                            CreateDateUtc = DateTime.UtcNow
                        };

                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// Currently acts as an interconnection between the new public api and the legacy api for publishing
    /// </summary>
    public class PublishingStrategy : BasePublishingStrategy
    {
       
        protected internal override Attempt<PublishStatus> PublishInternal(IContent content, int userId)
        {
            if (Publishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(content), this))
                return new Attempt<PublishStatus>(false, new PublishStatus(content, PublishStatusType.FailedCancelledByEvent));

            //Check if the Content is Expired to verify that it can in fact be published
            if (content.Status == ContentStatus.Expired)
            {
                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' has expired and could not be published.",
                                  content.Name, content.Id));
                return new Attempt<PublishStatus>(false, new PublishStatus(content, PublishStatusType.FailedHasExpired));
            }

            //Check if the Content is Awaiting Release to verify that it can in fact be published
            if (content.Status == ContentStatus.AwaitingRelease)
            {
                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' is awaiting release and could not be published.",
                                  content.Name, content.Id));
                return new Attempt<PublishStatus>(false, new PublishStatus(content, PublishStatusType.FailedAwaitingRelease));
            }

            //Check if the Content is Trashed to verify that it can in fact be published
            if (content.Status == ContentStatus.Trashed)
            {
                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' is trashed and could not be published.",
                                  content.Name, content.Id));
                return new Attempt<PublishStatus>(false, new PublishStatus(content, PublishStatusType.FailedIsTrashed));
            }

            content.ChangePublishedState(PublishedState.Published);

            LogHelper.Info<PublishingStrategy>(
                string.Format("Content '{0}' with Id '{1}' has been published.",
                              content.Name, content.Id));

            return new Attempt<PublishStatus>(true, new PublishStatus(content));
        }

        /// <summary>
        /// Publishes a single piece of Content
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to publish</param>
        /// <param name="userId">Id of the User issueing the publish operation</param>
        /// <returns>True if the publish operation was successfull and not cancelled, otherwise false</returns>
        public override bool Publish(IContent content, int userId)
        {
            return PublishInternal(content, userId).Success;
        }

        /// <summary>
        /// Publishes a list of content items
        /// </summary>
        /// <param name="content"></param>
        /// <param name="userId"></param>
        /// <param name="includeUnpublishedDocuments">
        /// By default this is set to true which means that it will publish any content item in the list that is completely unpublished and
        /// not visible on the front-end. If set to false, this will only publish content that is live on the front-end but has new versions
        /// that have yet to be published.
        /// </param>
        /// <returns></returns>
        protected internal override IEnumerable<Attempt<PublishStatus>> PublishWithChildrenInternal(IEnumerable<IContent> content, int userId, bool includeUnpublishedDocuments = true)
        {
            var statuses = new List<Attempt<PublishStatus>>();

            /* Only update content thats not already been published - we want to loop through
             * all unpublished content to write skipped content (expired and awaiting release) to log.
             */
            foreach (var item in content.Where(x => x.Published == false))
            {
                //Check if this item has never been published
                if (!includeUnpublishedDocuments && !item.HasPublishedVersion())
                {
                    //this item does not have a published version and the flag is set to not include them
                    continue;
                }

                //Fire Publishing event
                if (Publishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(item), this))

                    //Check if the Content is Expired to verify that it can in fact be published
                    if (item.Status == ContentStatus.Expired)
                    {
                        LogHelper.Info<PublishingStrategy>(
                            string.Format("Content '{0}' with Id '{1}' has expired and could not be published.",
                                          item.Name, item.Id));
                        statuses.Add(new Attempt<PublishStatus>(false, new PublishStatus(item, PublishStatusType.FailedHasExpired)));
                        continue;
                    }

                //Check if the Content is Awaiting Release to verify that it can in fact be published
                if (item.Status == ContentStatus.AwaitingRelease)
                {
                    LogHelper.Info<PublishingStrategy>(
                        string.Format("Content '{0}' with Id '{1}' is awaiting release and could not be published.",
                                      item.Name, item.Id));
                    statuses.Add(new Attempt<PublishStatus>(false, new PublishStatus(item, PublishStatusType.FailedAwaitingRelease)));
                    continue;
                }

                //Check if the Content is Trashed to verify that it can in fact be published
                if (item.Status == ContentStatus.Trashed)
                {
                    LogHelper.Info<PublishingStrategy>(
                        string.Format("Content '{0}' with Id '{1}' is trashed and could not be published.",
                                      item.Name, item.Id));
                    statuses.Add(new Attempt<PublishStatus>(false, new PublishStatus(item, PublishStatusType.FailedIsTrashed)));
                    continue;
                }

                item.ChangePublishedState(PublishedState.Published);

                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' has been published.",
                                  item.Name, item.Id));

                statuses.Add(new Attempt<PublishStatus>(true, new PublishStatus(item)));
            }

            return statuses;
        }

        /// <summary>
        /// Publishes a list of Content
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/></param>
        /// <param name="userId">Id of the User issueing the publish operation</param>
        /// <returns>True if the publish operation was successfull and not cancelled, otherwise false</returns>
        public override bool PublishWithChildren(IEnumerable<IContent> content, int userId)
        {
            var result = PublishWithChildrenInternal(content, userId);
            
            //NOTE: This previously always returned true so I've left it that way. It returned true because (from Morten)...
            // ... if one item couldn't be published it wouldn't be correct to return false.
            // in retrospect it should have returned a list of with Ids and Publish Status
            // come to think of it ... the cache would still be updated for a failed item or at least tried updated. 
            // It would call the Published event for the entire list, but if the Published property isn't set to True it 
            // wouldn't actually update the cache for that item. But not really ideal nevertheless...
            return true;
        }

        /// <summary>
        /// Unpublishes a single piece of Content
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to unpublish</param>
        /// <param name="userId">Id of the User issueing the unpublish operation</param>
        /// <returns>True if the unpublish operation was successfull and not cancelled, otherwise false</returns>
        public override bool UnPublish(IContent content, int userId)
        {
            if (UnPublishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(content), this))
                return false;

            //If Content has a release date set to before now, it should be removed so it doesn't interrupt an unpublish
            //Otherwise it would remain released == published
            if (content.ReleaseDate.HasValue && content.ReleaseDate.Value <= DateTime.Now)
            {
                content.ReleaseDate = null;

                LogHelper.Info<PublishingStrategy>(
                    string.Format(
                        "Content '{0}' with Id '{1}' had its release date removed, because it was unpublished.",
                        content.Name, content.Id));
            }
            
            content.ChangePublishedState(PublishedState.Unpublished);

            LogHelper.Info<PublishingStrategy>(
                string.Format("Content '{0}' with Id '{1}' has been unpublished.",
                              content.Name, content.Id));
            return true;
        }

        protected internal override IEnumerable<Attempt<PublishStatus>> UnPublishInternal(IEnumerable<IContent> content, int userId)
        {
            var result = new List<Attempt<PublishStatus>>();

            //Only update content thats already been published
            foreach (var item in content.Where(x => x.Published == true))
            {
                //Fire UnPublishing event
                if (UnPublishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(item), this))
                {
                    result.Add(new Attempt<PublishStatus>(false, new PublishStatus(item, PublishStatusType.FailedCancelledByEvent)));
                    continue;
                }

                //If Content has a release date set to before now, it should be removed so it doesn't interrupt an unpublish
                //Otherwise it would remain released == published
                if (item.ReleaseDate.HasValue && item.ReleaseDate.Value <= DateTime.Now)
                {
                    item.ReleaseDate = null;

                    LogHelper.Info<PublishingStrategy>(
                        string.Format("Content '{0}' with Id '{1}' had its release date removed, because it was unpublished.",
                                      item.Name, item.Id));
                }

                item.ChangePublishedState(PublishedState.Unpublished);

                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' has been unpublished.",
                                  item.Name, item.Id));

                result.Add(new Attempt<PublishStatus>(true, new PublishStatus(item)));
            }

            return result;
        }

        /// <summary>
        /// Unpublishes a list of Content
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/></param>
        /// <param name="userId">Id of the User issueing the unpublish operation</param>
        /// <returns>True if the unpublish operation was successfull and not cancelled, otherwise false</returns>
        public override bool UnPublish(IEnumerable<IContent> content, int userId)
        {
            var result = UnPublishInternal(content, userId);

            //NOTE: This previously always returned true so I've left it that way. It returned true because (from Morten)...
            // ... if one item couldn't be published it wouldn't be correct to return false.
            // in retrospect it should have returned a list of with Ids and Publish Status
            // come to think of it ... the cache would still be updated for a failed item or at least tried updated. 
            // It would call the Published event for the entire list, but if the Published property isn't set to True it 
            // wouldn't actually update the cache for that item. But not really ideal nevertheless...
            return true;
        }

        /// <summary>
        /// Call to fire event that updating the published content has finalized.
        /// </summary>
        /// <remarks>
        /// This seperation of the OnPublished event is done to ensure that the Content
        /// has been properly updated (committed unit of work) and xml saved in the db.
        /// </remarks>
        /// <param name="content"><see cref="IContent"/> thats being published</param>
        public override void PublishingFinalized(IContent content)
        {
            Published.RaiseEvent(new PublishEventArgs<IContent>(content, false, false), this);
        }

        /// <summary>
        /// Call to fire event that updating the published content has finalized.
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/> thats being published</param>
        /// <param name="isAllRepublished">Boolean indicating whether its all content that is republished</param>
        public override void PublishingFinalized(IEnumerable<IContent> content, bool isAllRepublished)
        {
            Published.RaiseEvent(new PublishEventArgs<IContent>(content, false, isAllRepublished), this);

        }

        /// <summary>
        /// Call to fire event that updating the unpublished content has finalized.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> thats being unpublished</param>
        public override void UnPublishingFinalized(IContent content)
        {
            UnPublished.RaiseEvent(new PublishEventArgs<IContent>(content, false, false), this);
        }

        /// <summary>
        /// Call to fire event that updating the unpublished content has finalized.
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/> thats being unpublished</param>
        public override void UnPublishingFinalized(IEnumerable<IContent> content)
        {
            UnPublished.RaiseEvent(new PublishEventArgs<IContent>(content, false, false), this);
        }

        /// <summary>
        /// Occurs before publish
        /// </summary>
        public static event TypedEventHandler<IPublishingStrategy, PublishEventArgs<IContent>> Publishing;

        /// <summary>
        /// Occurs after publish
        /// </summary>
        public static event TypedEventHandler<IPublishingStrategy, PublishEventArgs<IContent>> Published;
        
        /// <summary>
        /// Occurs before unpublish
        /// </summary>
        public static event TypedEventHandler<IPublishingStrategy, PublishEventArgs<IContent>> UnPublishing;

        /// <summary>
        /// Occurs after unpublish
        /// </summary>
        public static event TypedEventHandler<IPublishingStrategy, PublishEventArgs<IContent>> UnPublished;


    }
}
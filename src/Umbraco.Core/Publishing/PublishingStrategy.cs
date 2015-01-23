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

        /// <summary>
        /// Publishes a single piece of Content
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to publish</param>
        /// <param name="userId">Id of the User issueing the publish operation</param>        
        internal Attempt<PublishStatus> PublishInternal(IContent content, int userId)
        {
            if (Publishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(content), this))
            {
                LogHelper.Info<PublishingStrategy>(
                        string.Format("Content '{0}' with Id '{1}' will not be published, the event was cancelled.", content.Name, content.Id));
                return Attempt<PublishStatus>.Fail(new PublishStatus(content, PublishStatusType.FailedCancelledByEvent));
            }
                

            //Check if the Content is Expired to verify that it can in fact be published
            if (content.Status == ContentStatus.Expired)
            {
                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' has expired and could not be published.",
                                  content.Name, content.Id));
                return Attempt<PublishStatus>.Fail(new PublishStatus(content, PublishStatusType.FailedHasExpired));
            }

            //Check if the Content is Awaiting Release to verify that it can in fact be published
            if (content.Status == ContentStatus.AwaitingRelease)
            {
                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' is awaiting release and could not be published.",
                                  content.Name, content.Id));
                return Attempt<PublishStatus>.Fail(new PublishStatus(content, PublishStatusType.FailedAwaitingRelease));
            }

            //Check if the Content is Trashed to verify that it can in fact be published
            if (content.Status == ContentStatus.Trashed)
            {
                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' is trashed and could not be published.",
                                  content.Name, content.Id));
                return Attempt<PublishStatus>.Fail(new PublishStatus(content, PublishStatusType.FailedIsTrashed));
            }

            content.ChangePublishedState(PublishedState.Published);

            LogHelper.Info<PublishingStrategy>(
                string.Format("Content '{0}' with Id '{1}' has been published.",
                              content.Name, content.Id));

            return Attempt.Succeed(new PublishStatus(content));
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
        /// <remarks>
        /// 
        /// This method becomes complex once we start to be able to cancel events or stop publishing a content item in any way because if a
        /// content item is not published then it's children shouldn't be published either. This rule will apply for the following conditions:
        /// * If a document fails to be published, do not proceed to publish it's children if:
        /// ** The document does not have a publish version
        /// ** The document does have a published version but the includeUnpublishedDocuments = false
        /// 
        /// In order to do this, we will order the content by level and begin by publishing each item at that level, then proceed to the next
        /// level and so on. If we detect that the above rule applies when the document publishing is cancelled we'll add it to the list of 
        /// parentsIdsCancelled so that it's children don't get published.
        /// 
        /// Its important to note that all 'root' documents included in the list *will* be published regardless of the rules mentioned
        /// above (unless it is invalid)!! By 'root' documents we are referring to documents in the list with the minimum value for their 'level'. 
        /// In most cases the 'root' documents will only be one document since under normal circumstance we only publish one document and 
        /// its children. The reason we have to do this is because if a user is publishing a document and it's children, it is implied that
        /// the user definitely wants to publish it even if it has never been published before.
        /// 
        /// </remarks>
        internal IEnumerable<Attempt<PublishStatus>> PublishWithChildrenInternal(
            IEnumerable<IContent> content, int userId, bool includeUnpublishedDocuments = true)
        {
            var statuses = new List<Attempt<PublishStatus>>();

            //a list of all document ids that had their publishing cancelled during these iterations.
            //this helps us apply the rule listed in the notes above by checking if a document's parent id
            //matches one in this list.
            var parentsIdsCancelled = new List<int>();

            //group by levels and iterate over the sorted ascending level.
            //TODO: This will cause all queries to execute, they will not be lazy but I'm not really sure being lazy actually made
            // much difference because we iterate over them all anyways?? Morten?       
            // Because we're grouping I think this will execute all the queries anyways so need to fetch it all first.
            var fetchedContent = content.ToArray();
            
            //We're going to populate the statuses with all content that is already published because below we are only going to iterate over
            // content that is not published. We'll set the status to "AlreadyPublished"
            statuses.AddRange(fetchedContent.Where(x => x.Published)
                .Select(x => Attempt.Succeed(new PublishStatus(x, PublishStatusType.SuccessAlreadyPublished))));

            int? firstLevel = null;

            //group by level and iterate over each level (sorted ascending)
            var levelGroups = fetchedContent.GroupBy(x => x.Level);
            foreach (var level in levelGroups.OrderBy(x => x.Key))
            {
                //set the first level flag, used to ensure that all documents at the first level will 
                //be published regardless of the rules mentioned in the remarks.
                if (!firstLevel.HasValue)
                {
                    firstLevel = level.Key;    
                }

                /* Only update content thats not already been published - we want to loop through
                 * all unpublished content to write skipped content (expired and awaiting release) to log.
                 */
                foreach (var item in level.Where(x => x.Published == false))
                {
                    //Check if this item should be excluded because it's parent's publishing has failed/cancelled
                    if (parentsIdsCancelled.Contains(item.ParentId))
                    {
                        LogHelper.Info<PublishingStrategy>(
                            string.Format("Content '{0}' with Id '{1}' will not be published because it's parent's publishing action failed or was cancelled.", item.Name, item.Id));
                        //if this cannot be published, ensure that it's children can definitely not either!
                        parentsIdsCancelled.Add(item.Id);
                        continue;
                    }

                    //Check if this item has never been published (and that it is not at the root level)
                    if (item.Level != firstLevel && !includeUnpublishedDocuments && !item.HasPublishedVersion())
                    {
                        //this item does not have a published version and the flag is set to not include them
                        parentsIdsCancelled.Add(item.Id);
                        continue;
                    }

                    //Fire Publishing event
                    if (Publishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(item), this))
                    {
                        //the publishing has been cancelled.
                        LogHelper.Info<PublishingStrategy>(
                            string.Format("Content '{0}' with Id '{1}' will not be published, the event was cancelled.", item.Name, item.Id));
                        statuses.Add(Attempt.Fail(new PublishStatus(item, PublishStatusType.FailedCancelledByEvent)));

                        //Does this document apply to our rule to cancel it's children being published?
                        CheckCancellingOfChildPublishing(item, parentsIdsCancelled, includeUnpublishedDocuments);
                        
                        continue;
                    }

                    //Check if the content is valid if the flag is set to check
                    if (!item.IsValid())
                    {
                        LogHelper.Info<PublishingStrategy>(
                            string.Format("Content '{0}' with Id '{1}' will not be published because some of it's content is not passing validation rules.",
                                          item.Name, item.Id));
                        statuses.Add(Attempt.Fail(new PublishStatus(item, PublishStatusType.FailedContentInvalid)));

                        //Does this document apply to our rule to cancel it's children being published?
                        CheckCancellingOfChildPublishing(item, parentsIdsCancelled, includeUnpublishedDocuments);

                        continue;
                    }

                    //Check if the Content is Expired to verify that it can in fact be published
                    if (item.Status == ContentStatus.Expired)
                    {
                        LogHelper.Info<PublishingStrategy>(
                            string.Format("Content '{0}' with Id '{1}' has expired and could not be published.",
                                          item.Name, item.Id));
                        statuses.Add(Attempt.Fail(new PublishStatus(item, PublishStatusType.FailedHasExpired)));
                        
                        //Does this document apply to our rule to cancel it's children being published?
                        CheckCancellingOfChildPublishing(item, parentsIdsCancelled, includeUnpublishedDocuments);
                        
                        continue;
                    }

                    //Check if the Content is Awaiting Release to verify that it can in fact be published
                    if (item.Status == ContentStatus.AwaitingRelease)
                    {
                        LogHelper.Info<PublishingStrategy>(
                            string.Format("Content '{0}' with Id '{1}' is awaiting release and could not be published.",
                                          item.Name, item.Id));
                        statuses.Add(Attempt.Fail(new PublishStatus(item, PublishStatusType.FailedAwaitingRelease)));

                        //Does this document apply to our rule to cancel it's children being published?
                        CheckCancellingOfChildPublishing(item, parentsIdsCancelled, includeUnpublishedDocuments);

                        continue;
                    }

                    //Check if the Content is Trashed to verify that it can in fact be published
                    if (item.Status == ContentStatus.Trashed)
                    {
                        LogHelper.Info<PublishingStrategy>(
                            string.Format("Content '{0}' with Id '{1}' is trashed and could not be published.",
                                          item.Name, item.Id));
                        statuses.Add(Attempt.Fail(new PublishStatus(item, PublishStatusType.FailedIsTrashed)));

                        //Does this document apply to our rule to cancel it's children being published?
                        CheckCancellingOfChildPublishing(item, parentsIdsCancelled, includeUnpublishedDocuments);

                        continue;
                    }

                    item.ChangePublishedState(PublishedState.Published);

                    LogHelper.Info<PublishingStrategy>(
                        string.Format("Content '{0}' with Id '{1}' has been published.",
                                      item.Name, item.Id));

                    statuses.Add(Attempt.Succeed(new PublishStatus(item)));
                }
    
            }

            return statuses;
        }

        /// <summary>
        /// Based on the information provider we'll check if we should cancel the publishing of this document's children
        /// </summary>
        /// <param name="content"></param>
        /// <param name="parentsIdsCancelled"></param>
        /// <param name="includeUnpublishedDocuments"></param>
        /// <remarks>
        /// See remarks on method: PublishWithChildrenInternal
        /// </remarks> 
        private void CheckCancellingOfChildPublishing(IContent content, List<int> parentsIdsCancelled, bool includeUnpublishedDocuments)
        {
            //Does this document apply to our rule to cancel it's children being published?
            //TODO: We're going back to the service layer here... not sure how to avoid this? And this will add extra overhead to 
            // any document that fails to publish...
            var hasPublishedVersion = ApplicationContext.Current.Services.ContentService.HasPublishedVersion(content.Id);
            
            if (hasPublishedVersion && !includeUnpublishedDocuments)
            {
                //it has a published version but our flag tells us to not include un-published documents and therefore we should
                // not be forcing decendant/child documents to be published if their parent fails.
                parentsIdsCancelled.Add(content.Id);
            }
            else if (!hasPublishedVersion)
            {
                //it doesn't have a published version so we certainly cannot publish it's children.
                parentsIdsCancelled.Add(content.Id);                
            }
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
            return UnPublishInternal(content, userId).Success;
        }

        /// <summary>
        /// Unpublishes a list of Content
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/></param>
        /// <param name="userId">Id of the User issueing the unpublish operation</param>
        /// <returns>A list of publish statuses</returns>
        private IEnumerable<Attempt<PublishStatus>> UnPublishInternal(IEnumerable<IContent> content, int userId)
        {
            return content.Select(x => UnPublishInternal(x, userId));
        }

        private Attempt<PublishStatus> UnPublishInternal(IContent content, int userId)
        {
            // content should (is assumed to ) be the newest version, which may not be published
            // don't know how to test this, so it's not verified
            // NOTE
            // if published != newest, then the published flags need to be reseted by whoever is calling that method
            // at the moment it's done by the content service

            //Fire UnPublishing event
            if (UnPublishing.IsRaisedEventCancelled(new PublishEventArgs<IContent>(content), this))
            {
                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' will not be unpublished, the event was cancelled.", content.Name, content.Id));
                return Attempt.Fail(new PublishStatus(content, PublishStatusType.FailedCancelledByEvent));
            }

            //If Content has a release date set to before now, it should be removed so it doesn't interrupt an unpublish
            //Otherwise it would remain released == published
            if (content.ReleaseDate.HasValue && content.ReleaseDate.Value <= DateTime.Now)
            {
                content.ReleaseDate = null;

                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' had its release date removed, because it was unpublished.",
                                  content.Name, content.Id));
            }

            // if newest is published, unpublish
            if (content.Published)
                content.ChangePublishedState(PublishedState.Unpublished);

            LogHelper.Info<PublishingStrategy>(
                string.Format("Content '{0}' with Id '{1}' has been unpublished.",
                              content.Name, content.Id));

            return Attempt.Succeed(new PublishStatus(content));
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
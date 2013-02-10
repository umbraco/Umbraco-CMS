using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// Abstract class for the implementation of an <see cref="IPublishingStrategy"/>, which provides the events used for publishing/unpublishing.
    /// </summary>
    public abstract class BasePublishingStrategy : IPublishingStrategy
    {

        internal abstract Attempt<PublishStatus> PublishInternal(IContent content, int userId);

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
        /// <param name="validateContent">If true this will validate each content item before trying to publish it, if validation fails it will not be published.</param>
        /// <returns></returns>
        internal abstract IEnumerable<Attempt<PublishStatus>> PublishWithChildrenInternal(
            IEnumerable<IContent> content, int userId, bool includeUnpublishedDocuments = true, bool validateContent = false);

        internal abstract IEnumerable<Attempt<PublishStatus>> UnPublishInternal(IEnumerable<IContent> content, int userId);

        public abstract bool Publish(IContent content, int userId);
        public abstract bool PublishWithChildren(IEnumerable<IContent> content, int userId);
        public abstract bool UnPublish(IContent content, int userId);
        public abstract bool UnPublish(IEnumerable<IContent> content, int userId);

        /// <summary>
        /// Call to fire event that updating the published content has finalized.
        /// </summary>
        /// <remarks>
        /// This seperation of the OnPublished event is done to ensure that the Content
        /// has been properly updated (committed unit of work) and xml saved in the db.
        /// </remarks>
        /// <param name="content"><see cref="IContent"/> thats being published</param>
        public abstract void PublishingFinalized(IContent content);

        /// <summary>
        /// Call to fire event that updating the published content has finalized.
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/> thats being published</param>
        /// <param name="isAllRepublished">Boolean indicating whether its all content that is republished</param>
        public abstract void PublishingFinalized(IEnumerable<IContent> content, bool isAllRepublished);

        /// <summary>
        /// Call to fire event that updating the unpublished content has finalized.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> thats being unpublished</param>
        public abstract void UnPublishingFinalized(IContent content);

        /// <summary>
        /// Call to fire event that updating the unpublished content has finalized.
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/> thats being unpublished</param>
        public abstract void UnPublishingFinalized(IEnumerable<IContent> content);
    }
}
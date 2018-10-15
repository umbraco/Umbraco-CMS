using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models;

namespace Umbraco.Core.Publishing
{
    [Obsolete("This class is not intended to be used and will be removed in future versions, see IPublishingStrategy instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class BasePublishingStrategy : IPublishingStrategy
    {

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
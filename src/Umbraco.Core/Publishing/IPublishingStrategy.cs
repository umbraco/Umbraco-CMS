using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// Defines the Publishing Strategy
    /// </summary>
    public interface IPublishingStrategy
    {
        /// <summary>
        /// Publishes a single piece of Content
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to publish</param>
        /// <param name="userId">Id of the User issueing the publish operation</param>
        /// <returns>True if the publish operation was successfull and not cancelled, otherwise false</returns>
        bool Publish(IContent content, int userId);

        /// <summary>
        /// Publishes a list of Content
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/></param>
        /// <param name="userId">Id of the User issueing the publish operation</param>
        /// <returns>True if the publish operation was successfull and not cancelled, otherwise false</returns>
        bool PublishWithChildren(IEnumerable<IContent> content, int userId);

        /// <summary>
        /// Unpublishes a single piece of Content
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to unpublish</param>
        /// <param name="userId">Id of the User issueing the unpublish operation</param>
        /// <returns>True if the unpublish operation was successfull and not cancelled, otherwise false</returns>
        bool UnPublish(IContent content, int userId);

        /// <summary>
        /// Unpublishes a list of Content
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/></param>
        /// <param name="userId">Id of the User issueing the unpublish operation</param>
        /// <returns>True if the unpublish operation was successfull and not cancelled, otherwise false</returns>
        bool UnPublish(IEnumerable<IContent> content, int userId);

        /// <summary>
        /// Call to fire event that updating the published content has finalized.
        /// </summary>
        /// <remarks>
        /// This seperation of the OnPublished event is done to ensure that the Content
        /// has been properly updated (committed unit of work) and xml saved in the db.
        /// </remarks>
        /// <param name="content"><see cref="IContent"/> thats being published</param>
        void PublishingFinalized(IContent content);

        /// <summary>
        /// Call to fire event that updating the published content has finalized.
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/> thats being published</param>
        void PublishingFinalized(IEnumerable<IContent> content);

        /// <summary>
        /// Call to fire event that updating the unpublished content has finalized.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> thats being unpublished</param>
        void UnPublishingFinalized(IContent content);

        /// <summary>
        /// Call to fire event that updating the unpublished content has finalized.
        /// </summary>
        /// <param name="content">An enumerable list of <see cref="IContent"/> thats being unpublished</param>
        void UnPublishingFinalized(IEnumerable<IContent> content);
    }
}
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// TODO: This is a compatibility hack, we want to get rid of IPublishingStrategy all together or just have it as an internal
    /// helper class but we can't just remove it now, we also cannot just change it, so the current IPublishingStrategy one will simply not be used
    /// in our own code, if for some odd reason someone else is using it, then fine it will continue to work with hacks but won't be used by us.
    /// </summary>
    internal interface IPublishingStrategy2
    {
        /// <summary>
        /// Publishes a single piece of Content
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="content"><see cref="IContent"/> to publish</param>
        /// <param name="userId">Id of the User issueing the publish operation</param>
        /// <returns>True if the publish operation was successfull and not cancelled, otherwise false</returns>
        Attempt<PublishStatus> Publish(IScopeUnitOfWork uow, IContent content, int userId);

        /// <summary>
        /// Publishes a list of Content
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="content">An enumerable list of <see cref="IContent"/></param>
        /// <param name="userId">Id of the User issueing the publish operation</param>
        /// <param name="includeUnpublishedDocuments"></param>
        /// <returns>True if the publish operation was successfull and not cancelled, otherwise false</returns>
        IEnumerable<Attempt<PublishStatus>> PublishWithChildren(IScopeUnitOfWork uow, IEnumerable<IContent> content, int userId, bool includeUnpublishedDocuments);

        /// <summary>
        /// Unpublishes a single piece of Content
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="content"><see cref="IContent"/> to unpublish</param>
        /// <param name="userId">Id of the User issueing the unpublish operation</param>
        /// <returns>True if the unpublish operation was successfull and not cancelled, otherwise false</returns>
        Attempt<PublishStatus> UnPublish(IScopeUnitOfWork uow, IContent content, int userId);

        /// <summary>
        /// Call to fire event that updating the published content has finalized.
        /// </summary>
        /// <remarks>
        /// This seperation of the OnPublished event is done to ensure that the Content
        /// has been properly updated (committed unit of work) and xml saved in the db.
        /// </remarks>
        /// <param name="uow"></param>
        /// <param name="content"><see cref="IContent"/> thats being published</param>
        void PublishingFinalized(IScopeUnitOfWork uow, IContent content);

        /// <summary>
        /// Call to fire event that updating the published content has finalized.
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="content">An enumerable list of <see cref="IContent"/> thats being published</param>
        /// <param name="isAllRepublished">Boolean indicating whether its all content that is republished</param>
        void PublishingFinalized(IScopeUnitOfWork uow, IEnumerable<IContent> content, bool isAllRepublished);

        /// <summary>
        /// Call to fire event that updating the unpublished content has finalized.
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="content"><see cref="IContent"/> thats being unpublished</param>
        void UnPublishingFinalized(IScopeUnitOfWork uow, IContent content);

        /// <summary>
        /// Call to fire event that updating the unpublished content has finalized.
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="content">An enumerable list of <see cref="IContent"/> thats being unpublished</param>
        void UnPublishingFinalized(IScopeUnitOfWork uow, IEnumerable<IContent> content);
    }

    /// <summary>
    /// TODO: This should be obsoleted but if we did that then the Publish/Unpublish events on the content service would show that the param is obsoleted
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
        /// <param name="isAllRepublished">Boolean indicating whether its all content that is republished</param>
        void PublishingFinalized(IEnumerable<IContent> content, bool isAllRepublished);

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
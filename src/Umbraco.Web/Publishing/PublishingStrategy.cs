using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;

namespace Umbraco.Web.Publishing
{
    /// <summary>
    /// Currently acts as an interconnection between the new public api and the legacy api for publishing
    /// </summary>
    internal class PublishingStrategy : BasePublishingStrategy
    {
        internal PublishingStrategy()
        {
        }

        /// <summary>
        /// Publishes a single piece of content
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to publish</param>
        /// <param name="userId">Id of the user issueing the publish</param>
        /// <returns>True if the content was published, otherwise false</returns>
        public override bool Publish(IContent content, int userId)
        {
            //Fire BeforePublish event
            var e = new PublishEventArgs();
            FireBeforePublish(content, e);

            if (!e.Cancel)
            {
                content.ChangePublishedState(true);

                //Fire AfterPublish event
                FireAfterPublish(content, e);

                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' has been published.",
                                  content.Name, content.Id));

                //NOTE: Ideally the xml cache should be refreshed here - as part of the publishing

                return true;
            }

            return false;
        }

        /// <summary>
        /// Publishes a list of content
        /// </summary>
        /// <param name="children">List of <see cref="IContent"/> to publish</param>
        /// <param name="userId">Id of the user issueing the publish</param>
        /// <returns>True if the content was published, otherwise false</returns>
        public override bool PublishWithChildren(IEnumerable<IContent> children, int userId)
        {
            //Fire BeforePublish event
            var e = new PublishEventArgs();

            if (e.Cancel)
                return false;

            //Only update content thats not already been published
            foreach (var content in children.Where(x => x.Published == false))
            {
                FireBeforePublish(content, e);

                content.ChangePublishedState(true);
                
                //Fire AfterPublish event
                FireAfterPublish(content, e);

                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' has been published.",
                                  content.Name, content.Id));
            }

            //NOTE: Ideally the xml cache should be refreshed here - as part of the publishing

            return true;
        }

        /// <summary>
        /// Unpublishes a single piece of content
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to unpublish</param>
        /// <param name="userId">Id of the user issueing the unpublish</param>
        /// <returns>True is the content was unpublished, otherwise false</returns>
        public override bool UnPublish(IContent content, int userId)
        {
            var e = new UnPublishEventArgs();
            FireBeforeUnPublish(content, e);

            if (!e.Cancel)
            {
                content.ChangePublishedState(false);
                
                FireAfterUnPublish(content, e);

                LogHelper.Info<PublishingStrategy>(
                    string.Format("Content '{0}' with Id '{1}' has been unpublished.",
                                  content.Name, content.Id));

                //NOTE: Ideally the xml cache should be refreshed here - as part of the unpublishing

                return true;
            }

            return false;
        }
    }
}
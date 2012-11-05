using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Web.Publishing
{
    public abstract class BasePublishingStrategy : IPublishingStrategy
    {
        public abstract bool Publish(IContent content, int userId);
        public abstract bool PublishWithChildren(IEnumerable<IContent> content, int userId);
        public abstract bool UnPublish(IContent content, int userId);
        public abstract bool UnPublish(IEnumerable<IContent> content, int userId);

        /// <summary>
        /// The publishing event handler used for publish and unpublish events
        /// </summary>
        public delegate void PublishingEventHandler(IContent sender, PublishingEventArgs e);

        /// <summary>
        /// Occurs before publish
        /// </summary>
        public static event PublishingEventHandler Publishing;

        /// <summary>
        /// Raises the <see cref="Publishing"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnPublish(IContent content, PublishingEventArgs e)
        {
            if (Publishing != null)
                Publishing(content, e);
        }

        /// <summary>
        /// Occurs after publish
        /// </summary>
        public static event PublishingEventHandler Published;

        /// <summary>
        /// Raises the <see cref="Published"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnPublished(IContent content, PublishingEventArgs e)
        {
            if (Published != null)
                Published(content, e);
        }

        /// <summary>
        /// Occurs before unpublish
        /// </summary>
        public static event PublishingEventHandler UnPublishing;

        /// <summary>
        /// Raises the <see cref="UnPublishing"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnUnPublish(IContent content, PublishingEventArgs e)
        {
            if (UnPublishing != null)
                UnPublishing(content, e);
        }

        /// <summary>
        /// Occurs after unpublish
        /// </summary>
        public static event PublishingEventHandler UnPublished;

        /// <summary>
        /// Raises the <see cref="UnPublished"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnUnPublished(IContent content, PublishingEventArgs e)
        {
            if (UnPublished != null)
                UnPublished(content, e);
        }
    }
}
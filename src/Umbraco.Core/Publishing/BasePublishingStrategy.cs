using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Publishing
{
    /// <summary>
    /// Abstract class for the implementation of an <see cref="IPublishingStrategy"/>, which provides the events used for publishing/unpublishing.
    /// </summary>
    public abstract class BasePublishingStrategy : IPublishingStrategy
    {
        public abstract bool Publish(IContent content, int userId);
        public abstract bool PublishWithChildren(IEnumerable<IContent> content, int userId);
        public abstract bool UnPublish(IContent content, int userId);
        public abstract bool UnPublish(IEnumerable<IContent> content, int userId);

        /// <summary>
        /// Occurs before publish
        /// </summary>
        public static event EventHandler<PublishingEventArgs> Publishing;

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
        public static event EventHandler<PublishingEventArgs> Published;

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
        /// Raises the <see cref="Published"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnPublished(IEnumerable<IContent> content, PublishingEventArgs e)
        {
            if (Published != null)
                Published(content, e);
        }

        /// <summary>
        /// Occurs before unpublish
        /// </summary>
        public static event EventHandler<PublishingEventArgs> UnPublishing;

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
        public static event EventHandler<PublishingEventArgs> UnPublished;

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

        /// <summary>
        /// Raises the <see cref="UnPublished"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnUnPublished(IEnumerable<IContent> content, PublishingEventArgs e)
        {
            if (UnPublished != null)
                UnPublished(content, e);
        }
    }
}
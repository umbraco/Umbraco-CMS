using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Web.Publishing
{
    public abstract class BasePublishingStrategy : IPublishingStrategy
    {
        public abstract bool Publish(IContent content, int userId);
        public abstract bool PublishWithChildren(IEnumerable<IContent> children, int userId);
        public abstract bool UnPublish(IContent content, int userId);

        /// <summary>
        /// The publish event handler
        /// </summary>
        public delegate void PublishEventHandler(IContent sender, PublishEventArgs e);

        /// <summary>
        /// The unpublish event handler
        /// </summary>
        public delegate void UnPublishEventHandler(IContent sender, UnPublishEventArgs e);

        /// <summary>
        /// Occurs before publish
        /// </summary>
        public static event PublishEventHandler BeforePublish;

        /// <summary>
        /// Raises the <see cref="E:BeforePublish"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforePublish(IContent content, PublishEventArgs e)
        {
            if (BeforePublish != null)
                BeforePublish(content, e);
        }

        /// <summary>
        /// Occurs after publish
        /// </summary>
        public static event PublishEventHandler AfterPublish;

        /// <summary>
        /// Raises the <see cref="E:AfterPublish"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterPublish(IContent content, PublishEventArgs e)
        {
            if (AfterPublish != null)
                AfterPublish(content, e);
        }

        /// <summary>
        /// Occurs before unpublish
        /// </summary>
        public static event UnPublishEventHandler BeforeUnPublish;

        /// <summary>
        /// Raises the <see cref="E:BeforeUnPublish"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeUnPublish(IContent content, UnPublishEventArgs e)
        {
            if (BeforeUnPublish != null)
                BeforeUnPublish(content, e);
        }

        /// <summary>
        /// Occurs after unpublish
        /// </summary>
        public static event UnPublishEventHandler AfterUnPublish;

        /// <summary>
        /// Raises the <see cref="E:AfterUnPublish"/> event
        /// </summary>
        /// <param name="content"> </param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterUnPublish(IContent content, UnPublishEventArgs e)
        {
            if (AfterUnPublish != null)
                AfterUnPublish(content, e);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IPublishedModelFactory"/>.
    /// </summary>
    public static class PublishedModelFactoryExtensions
    {
        /// <summary>
        /// Returns true if the current <see cref="IPublishedModelFactory"/> is an implementation of <see cref="ILivePublishedModelFactory"/>
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static bool IsLiveFactory(this IPublishedModelFactory factory) => factory is ILivePublishedModelFactory;

        /// <summary>
        /// Returns true if the current <see cref="IPublishedModelFactory"/> is an implementation of <see cref="ILivePublishedModelFactory2"/> and is enabled
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static bool IsLiveFactoryEnabled(this IPublishedModelFactory factory)
        {
            if (factory is ILivePublishedModelFactory liveFactory)
                return liveFactory.Enabled;

            // if it's not ILivePublishedModelFactory we can't determine if it's enabled or not so return true
            return true;
        }

        [Obsolete("This method is no longer used or necessary and will be removed from future")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void WithSafeLiveFactory(this IPublishedModelFactory factory, Action action)
        {
            if (factory is ILivePublishedModelFactory liveFactory)
            {
                lock (liveFactory.SyncRoot)
                {
                    //Call refresh on the live factory to re-compile the models
                    liveFactory.Refresh();
                    action();
                }
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Sets a flag to reset the ModelsBuilder models if the <see cref="IPublishedModelFactory"/> is <see cref="ILivePublishedModelFactory"/>
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="action"></param>
        /// <remarks>
        /// This does not recompile the pure live models, only sets a flag to tell models builder to recompile when they are requested.
        /// </remarks>
        internal static void WithSafeLiveFactoryReset(this IPublishedModelFactory factory, Action action)
        {
            if (factory is ILivePublishedModelFactory liveFactory)
            {
                lock (liveFactory.SyncRoot)
                {
                    liveFactory.Reset();

                    action();
                }
            }
            else
            {
                action();
            }
        }

    }
}

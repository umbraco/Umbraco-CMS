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
                    // TODO: Fix this in 8.3! - We need to change the ILivePublishedModelFactory interface to have a Reset method and then when we have an embedded MB
                    // version we will publicize the ResetModels (and change the name to Reset).
                    // For now, this will suffice and we'll use reflection, there should be no other implementation of ILivePublishedModelFactory.
                    // Calling ResetModels resets the MB flag so that the next time EnsureModels is called (which is called when nucache lazily calls CreateModel) it will
                    // trigger the recompiling of pure live models.
                    var resetMethod = liveFactory.GetType().GetMethod("ResetModels", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (resetMethod != null)
                        resetMethod.Invoke(liveFactory, null);

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

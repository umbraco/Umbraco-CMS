using System;
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
        /// Executes an action with a safe live factory
        /// </summary>
        /// <remarks>
        /// <para>If the factory is a live factory, ensures it is refreshed and locked while executing the action.</para>
        /// </remarks>
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
    }
}

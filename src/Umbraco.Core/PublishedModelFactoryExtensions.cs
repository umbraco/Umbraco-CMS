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
        /// Executes an action with a safe live factory/
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

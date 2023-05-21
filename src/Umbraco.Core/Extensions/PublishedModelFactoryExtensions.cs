// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IPublishedModelFactory" />.
/// </summary>
public static class PublishedModelFactoryExtensions
{
    /// <summary>
    ///     Returns true if the current <see cref="IPublishedModelFactory" /> is an implementation of
    ///     <see cref="IAutoPublishedModelFactory" /> and is enabled
    /// </summary>
    public static bool IsLiveFactoryEnabled(this IPublishedModelFactory factory)
    {
        if (factory is IAutoPublishedModelFactory liveFactory)
        {
            return liveFactory.Enabled;
        }

        // if it's not ILivePublishedModelFactory we know we're not using a live factory
        return false;
    }

    /// <summary>
    ///     Sets a flag to reset the ModelsBuilder models if the <see cref="IPublishedModelFactory" /> is
    ///     <see cref="IAutoPublishedModelFactory" />
    /// </summary>
    /// <remarks>
    ///     This does not recompile the InMemory models, only sets a flag to tell models builder to recompile when they are
    ///     requested.
    /// </remarks>
    internal static void WithSafeLiveFactoryReset(this IPublishedModelFactory factory, Action action)
    {
        if (factory is IAutoPublishedModelFactory liveFactory)
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

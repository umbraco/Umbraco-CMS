// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications related to content type cache refresh operations.
/// </summary>
/// <typeparam name="T">The type of content type composition.</typeparam>
/// <remarks>
///     This notification is used internally for cache synchronization when content types are modified.
/// </remarks>
public abstract class ContentTypeRefreshNotification<T> : ContentTypeChangeNotification<T>
    where T : class, IContentTypeComposition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeRefreshNotification{T}"/> class
    ///     with a single content type change.
    /// </summary>
    /// <param name="target">The content type change that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    protected ContentTypeRefreshNotification(ContentTypeChange<T> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeRefreshNotification{T}"/> class
    ///     with multiple content type changes.
    /// </summary>
    /// <param name="target">The collection of content type changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    protected ContentTypeRefreshNotification(IEnumerable<ContentTypeChange<T>> target, EventMessages messages)
        : base(
        target, messages)
    {
    }
}

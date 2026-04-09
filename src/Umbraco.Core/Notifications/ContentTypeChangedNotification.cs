// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is published when a ContentType is saved or deleted, after the transaction has completed.
///     This is mainly used for caching purposes, and generally not recommended. Use <see cref="ContentTypeSavedNotification"/> and <see cref="ContentTypeDeletedNotification"/> instead.
/// </summary>
public class ContentTypeChangedNotification : ContentTypeChangeNotification<IContentType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeChangedNotification"/> class
    ///     with a single content type change.
    /// </summary>
    /// <param name="target">The content type change that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeChangedNotification(ContentTypeChange<IContentType> target, EventMessages messages)
        : base(
        target,
        messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeChangedNotification"/> class
    ///     with multiple content type changes.
    /// </summary>
    /// <param name="target">The content type changes that occurred.</param>
    /// <param name="messages">The event messages collection.</param>
    public ContentTypeChangedNotification(IEnumerable<ContentTypeChange<IContentType>> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

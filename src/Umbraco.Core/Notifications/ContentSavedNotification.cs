// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the IContentService when the Save method is called in the API and after the data has been persisted.
/// </summary>
public sealed class ContentSavedNotification : SavedNotification<IContent>
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="ContentSavedNotification"/>
    /// </summary>
    public ContentSavedNotification(IContent target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Gets a enumeration of <see cref="IContent"/>.
    /// </summary>
    public ContentSavedNotification(IEnumerable<IContent> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

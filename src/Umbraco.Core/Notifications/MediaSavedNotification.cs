// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaService when the Save method is called in the API and after the data has been persisted.
/// </summary>
public sealed class MediaSavedNotification : SavedNotification<IMedia>
{
    public MediaSavedNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSavedNotification"/>.
    /// </summary>
    /// <param name="target">
    ///  Gets the saved collection of <see cref="IMedia"/> objects.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    public MediaSavedNotification(IEnumerable<IMedia> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

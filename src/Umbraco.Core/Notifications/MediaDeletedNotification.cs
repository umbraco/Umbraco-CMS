// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the IMediaService when the Delete and EmptyRecycleBin methods are called in the API, after the media has been deleted.
/// </summary>
public sealed class MediaDeletedNotification : DeletedNotification<IMedia>
{
    public MediaDeletedNotification(IMedia target, EventMessages messages)
        : base(target, messages)
    {
    }
}

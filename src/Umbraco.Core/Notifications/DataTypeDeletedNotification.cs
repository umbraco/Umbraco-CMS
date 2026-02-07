// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a data type has been deleted.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IDataTypeService"/> after the data type has been removed.
///     It is not cancelable since the delete operation has already completed.
/// </remarks>
public class DataTypeDeletedNotification : DeletedNotification<IDataType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeDeletedNotification"/> class with a single data type.
    /// </summary>
    /// <param name="target">The data type that was deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public DataTypeDeletedNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }
}

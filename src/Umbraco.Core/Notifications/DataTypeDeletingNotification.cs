// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a data type is deleted.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the delete operation.
///     The notification is published by the <see cref="Services.IDataTypeService"/> before the data type is removed.
/// </remarks>
public class DataTypeDeletingNotification : DeletingNotification<IDataType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeDeletingNotification"/> class with a single data type.
    /// </summary>
    /// <param name="target">The data type being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public DataTypeDeletingNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }
}

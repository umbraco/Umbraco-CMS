// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published before a data type is saved.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation.
///     The notification is published by the <see cref="Services.IDataTypeService"/> before the data type is persisted.
/// </remarks>
public class DataTypeSavingNotification : SavingNotification<IDataType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeSavingNotification"/> class with a single data type.
    /// </summary>
    /// <param name="target">The data type being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DataTypeSavingNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeSavingNotification"/> class with multiple data types.
    /// </summary>
    /// <param name="target">The collection of data types being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DataTypeSavingNotification(IEnumerable<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

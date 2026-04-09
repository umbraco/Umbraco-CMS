// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a data type has been saved.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IDataTypeService"/> after the data type has been persisted.
///     It is not cancelable since the save operation has already completed.
/// </remarks>
public class DataTypeSavedNotification : SavedNotification<IDataType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeSavedNotification"/> class with a single data type.
    /// </summary>
    /// <param name="target">The data type that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DataTypeSavedNotification(IDataType target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeSavedNotification"/> class with multiple data types.
    /// </summary>
    /// <param name="target">The collection of data types that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DataTypeSavedNotification(IEnumerable<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

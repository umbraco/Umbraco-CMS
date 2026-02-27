// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the DataTypeService when the Move method is called in the API.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the move operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class DataTypeMovingNotification : MovingNotification<IDataType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeMovingNotification"/> class.
    /// </summary>
    /// <param name="target">The move event information for the data type being moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DataTypeMovingNotification(MoveEventInfo<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the DataTypeService when the Move method is called in the API, after the IDataType has been moved.
/// </summary>
public class DataTypeMovedNotification : MovedNotification<IDataType>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeMovedNotification"/> class.
    /// </summary>
    /// <param name="target">The move event information for the data type that was moved.</param>
    /// <param name="messages">The event messages collection.</param>
    public DataTypeMovedNotification(MoveEventInfo<IDataType> target, EventMessages messages)
        : base(target, messages)
    {
    }
}

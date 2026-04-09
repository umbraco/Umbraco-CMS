// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for cancelable notifications that carry a target object.
/// </summary>
/// <typeparam name="T">The type of the target object associated with this notification.</typeparam>
/// <remarks>
///     This class combines the functionality of <see cref="ObjectNotification{T}"/> and
///     <see cref="ICancelableNotification"/>, allowing handlers to both access the target object
///     and cancel the operation if needed.
/// </remarks>
public abstract class CancelableObjectNotification<T> : ObjectNotification<T>, ICancelableNotification
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CancelableObjectNotification{T}"/> class.
    /// </summary>
    /// <param name="target">The target object associated with this notification.</param>
    /// <param name="messages">The event messages collection.</param>
    protected CancelableObjectNotification(T target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <inheritdoc />
    public bool Cancel { get; set; }

    /// <summary>
    ///     Cancels the operation and adds a cancellation message to the event messages collection.
    /// </summary>
    /// <param name="cancelationMessage">The message explaining why the operation was canceled.</param>
    public void CancelOperation(EventMessage cancelationMessage)
    {
        Cancel = true;
        cancelationMessage.IsDefaultEventMessage = true;
        Messages.Add(cancelationMessage);
    }
}

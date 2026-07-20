// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Represents a notification that can be canceled by a notification handler.
/// </summary>
/// <remarks>
///     When a notification handler sets <see cref="Cancel"/> to <c>true</c>,
///     the operation that triggered the notification will be aborted.
/// </remarks>
public interface ICancelableNotification : INotification
{
    /// <summary>
    ///     Gets or sets a value indicating whether the operation should be canceled.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the operation should be canceled; otherwise, <c>false</c>.
    /// </value>
    bool Cancel { get; set; }
}

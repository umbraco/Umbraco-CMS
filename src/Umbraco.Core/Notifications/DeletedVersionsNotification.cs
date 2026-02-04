// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications published after versions have been deleted.
/// </summary>
/// <typeparam name="T">The type of entity whose versions were deleted.</typeparam>
/// <remarks>
///     This notification is published after versions have been successfully deleted,
///     allowing handlers to react to the deletion for auditing or cache invalidation purposes.
/// </remarks>
public abstract class DeletedVersionsNotification<T> : DeletedVersionsNotificationBase<T>
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletedVersionsNotification{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the entity whose versions were deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="specificVersion">The specific version that was deleted, or default if based on other criteria.</param>
    /// <param name="deletePriorVersions">Whether all versions prior to the specified version were deleted.</param>
    /// <param name="dateToRetain">The date before which versions were deleted.</param>
    protected DeletedVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
        : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }
}

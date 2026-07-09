// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for cancelable notifications published before versions are deleted.
/// </summary>
/// <typeparam name="T">The type of entity whose versions are being deleted.</typeparam>
/// <remarks>
///     This notification is published before versions are deleted, allowing handlers to cancel
///     the operation by setting <see cref="Cancel"/> to <c>true</c>.
/// </remarks>
public abstract class DeletingVersionsNotification<T> : DeletedVersionsNotificationBase<T>, ICancelableNotification
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletingVersionsNotification{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the entity whose versions are being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="specificVersion">The specific version to delete, or default to delete based on other criteria.</param>
    /// <param name="deletePriorVersions">Whether to delete all versions prior to the specified version.</param>
    /// <param name="dateToRetain">The date before which versions should be deleted.</param>
    protected DeletingVersionsNotification(int id, EventMessages messages, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
        : base(id, messages, specificVersion, deletePriorVersions, dateToRetain)
    {
    }

    /// <inheritdoc />
    public bool Cancel { get; set; }
}

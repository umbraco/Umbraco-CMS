// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications related to deleting content or media versions.
/// </summary>
/// <typeparam name="T">The type of entity whose versions are being deleted.</typeparam>
/// <remarks>
///     This notification provides information about which versions are being deleted,
///     including support for deleting specific versions, prior versions, or versions
///     before a certain date.
/// </remarks>
public abstract class DeletedVersionsNotificationBase<T> : StatefulNotification
    where T : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeletedVersionsNotificationBase{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the entity whose versions are being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    /// <param name="specificVersion">The specific version to delete, or default to delete based on other criteria.</param>
    /// <param name="deletePriorVersions">Whether to delete all versions prior to the specified version.</param>
    /// <param name="dateToRetain">The date before which versions should be deleted.</param>
    protected DeletedVersionsNotificationBase(
        int id,
        EventMessages messages,
        int specificVersion = default,
        bool deletePriorVersions = false,
        DateTime dateToRetain = default)
    {
        Id = id;
        Messages = messages;
        SpecificVersion = specificVersion;
        DeletePriorVersions = deletePriorVersions;
        DateToRetain = dateToRetain;
    }

    /// <summary>
    ///     Gets the ID of the entity whose versions are being deleted.
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Gets the event messages collection.
    /// </summary>
    public EventMessages Messages { get; }

    /// <summary>
    ///     Gets the specific version to delete, or default if not targeting a specific version.
    /// </summary>
    public int SpecificVersion { get; }

    /// <summary>
    ///     Gets a value indicating whether to delete all versions prior to the specified version.
    /// </summary>
    public bool DeletePriorVersions { get; }

    /// <summary>
    ///     Gets the date before which versions should be deleted.
    /// </summary>
    public DateTime DateToRetain { get; }
}

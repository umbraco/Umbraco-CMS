namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The action associated with saving a content item.
/// </summary>
public enum ContentSaveAction
{
    /// <summary>
    ///     Saves the content item, no publish.
    /// </summary>
    Save = 0,

    /// <summary>
    ///     Creates a new content item.
    /// </summary>
    SaveNew = 1,

    /// <summary>
    ///     Saves and publishes the content item.
    /// </summary>
    Publish = 2,

    /// <summary>
    ///     Creates and publishes a new content item.
    /// </summary>
    PublishNew = 3,

    /// <summary>
    ///     Saves and sends publish notification.
    /// </summary>
    SendPublish = 4,

    /// <summary>
    ///     Creates and sends publish notification.
    /// </summary>
    SendPublishNew = 5,

    /// <summary>
    ///     Saves and schedules publishing.
    /// </summary>
    Schedule = 6,

    /// <summary>
    ///     Creates and schedules publishing.
    /// </summary>
    ScheduleNew = 7,

    /// <summary>
    ///     Saves and publishes the content item including all descendants that have a published version.
    /// </summary>
    PublishWithDescendants = 8,

    /// <summary>
    ///     Creates and publishes the new content item including all descendants that have a published version.
    /// </summary>
    PublishWithDescendantsNew = 9,

    /// <summary>
    ///     Saves and publishes the content item including all descendants regardless of whether they have a published version
    ///     or not.
    /// </summary>
    [Obsolete("This option is no longer used as the 'force' aspect has been extended into options for publishing unpublished and re-publishing changed content. Please use one of those options instead.")]
    PublishWithDescendantsForce = 10,

    /// <summary>
    ///     Creates and publishes the new content item including all descendants regardless of whether they have a published
    ///     version or not.
    /// </summary>
    [Obsolete("This option is no longer used as the 'force' aspect has been extended into options for publishing unpublished and re-publishing changed content. Please use one of those options instead.")]
    PublishWithDescendantsForceNew = 11,

    /// <summary>
    ///     Saves and publishes the content item including all descendants including publishing previously unpublished content.
    /// </summary>
    PublishWithDescendantsIncludeUnpublished = 12,

    /// <summary>
    ///     Saves and publishes the new content item including all descendants including publishing previously unpublished content.
    /// </summary>
    PublishWithDescendantsIncludeUnpublishedNew = 13,

    /// <summary>
    ///     Saves and publishes the content item including all descendants irrespective of whether there are any pending changes.
    /// </summary>
    PublishWithDescendantsForceRepublish = 14,

    /// <summary>
    ///     Saves and publishes the new content item including all descendants including publishing previously unpublished content.
    /// </summary>
    PublishWithDescendantsForceRepublishNew = 15,

    /// <summary>
    ///     Saves and publishes the content item including all descendants including publishing previously unpublished content and irrespective of whether there are any pending changes.
    /// </summary>
    PublishWithDescendantsIncludeUnpublishedAndForceRepublish = 16,

    /// <summary>
    ///     Saves and publishes the new content item including all descendants including publishing previously unpublished content and irrespective of whether there are any pending changes.
    /// </summary>
    PublishWithDescendantsIncludeUnpublishedAndForceRepublishNew = 17,
}

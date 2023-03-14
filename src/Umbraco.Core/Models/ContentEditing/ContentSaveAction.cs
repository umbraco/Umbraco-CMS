namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     The action associated with saving a content item
/// </summary>
public enum ContentSaveAction
{
    /// <summary>
    ///     Saves the content item, no publish
    /// </summary>
    Save = 0,

    /// <summary>
    ///     Creates a new content item
    /// </summary>
    SaveNew = 1,

    /// <summary>
    ///     Saves and publishes the content item
    /// </summary>
    Publish = 2,

    /// <summary>
    ///     Creates and publishes a new content item
    /// </summary>
    PublishNew = 3,

    /// <summary>
    ///     Saves and sends publish notification
    /// </summary>
    SendPublish = 4,

    /// <summary>
    ///     Creates and sends publish notification
    /// </summary>
    SendPublishNew = 5,

    /// <summary>
    ///     Saves and schedules publishing
    /// </summary>
    Schedule = 6,

    /// <summary>
    ///     Creates and schedules publishing
    /// </summary>
    ScheduleNew = 7,

    /// <summary>
    ///     Saves and publishes the content item including all descendants that have a published version
    /// </summary>
    PublishWithDescendants = 8,

    /// <summary>
    ///     Creates and publishes the content item including all descendants that have a published version
    /// </summary>
    PublishWithDescendantsNew = 9,

    /// <summary>
    ///     Saves and publishes the content item including all descendants regardless of whether they have a published version
    ///     or not
    /// </summary>
    PublishWithDescendantsForce = 10,

    /// <summary>
    ///     Creates and publishes the content item including all descendants regardless of whether they have a published
    ///     version or not
    /// </summary>
    PublishWithDescendantsForceNew = 11,
}

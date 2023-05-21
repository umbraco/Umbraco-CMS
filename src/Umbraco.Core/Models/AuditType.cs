namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines audit types.
/// </summary>
public enum AuditType
{
    /// <summary>
    ///     New node(s) being added.
    /// </summary>
    New,

    /// <summary>
    ///     Node(s) being saved.
    /// </summary>
    Save,

    /// <summary>
    ///     Variant(s) being saved.
    /// </summary>
    SaveVariant,

    /// <summary>
    ///     Node(s) being opened.
    /// </summary>
    Open,

    /// <summary>
    ///     Node(s) being deleted.
    /// </summary>
    Delete,

    /// <summary>
    ///     Node(s) being published.
    /// </summary>
    Publish,

    /// <summary>
    ///     Variant(s) being published.
    /// </summary>
    PublishVariant,

    /// <summary>
    ///     Node(s) being sent to publishing.
    /// </summary>
    SendToPublish,

    /// <summary>
    ///     Variant(s) being sent to publishing.
    /// </summary>
    SendToPublishVariant,

    /// <summary>
    ///     Node(s) being unpublished.
    /// </summary>
    Unpublish,

    /// <summary>
    ///     Variant(s) being unpublished.
    /// </summary>
    UnpublishVariant,

    /// <summary>
    ///     Node(s) being moved.
    /// </summary>
    Move,

    /// <summary>
    ///     Node(s) being copied.
    /// </summary>
    Copy,

    /// <summary>
    ///     Node(s) being assigned domains.
    /// </summary>
    AssignDomain,

    /// <summary>
    ///     Node(s) public access changing.
    /// </summary>
    PublicAccess,

    /// <summary>
    ///     Node(s) being sorted.
    /// </summary>
    Sort,

    /// <summary>
    ///     Notification(s) being sent to user.
    /// </summary>
    Notify,

    /// <summary>
    ///     General system audit message.
    /// </summary>
    System,

    /// <summary>
    ///     Node's content being rolled back to a previous version.
    /// </summary>
    RollBack,

    /// <summary>
    ///     Package being installed.
    /// </summary>
    PackagerInstall,

    /// <summary>
    ///     Package being uninstalled.
    /// </summary>
    PackagerUninstall,

    /// <summary>
    ///     Custom audit message.
    /// </summary>
    Custom,

    /// <summary>
    ///     Content version preventCleanup set to true
    /// </summary>
    ContentVersionPreventCleanup,

    /// <summary>
    ///     Content version preventCleanup set to false
    /// </summary>
    ContentVersionEnableCleanup,
}

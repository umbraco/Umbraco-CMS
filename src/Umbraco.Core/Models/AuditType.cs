namespace Umbraco.Core.Models
{
    /// <summary>
    /// Enums for vailable types of auditing
    /// </summary>
    public enum AuditType
    {
        /// <summary>
        /// Used when new nodes are added
        /// </summary>
        New,
        /// <summary>
        /// Used when nodes are saved
        /// </summary>
        Save,
        /// <summary>
        /// Used when nodes are opened
        /// </summary>
        Open,
        /// <summary>
        /// Used when nodes are deleted
        /// </summary>
        Delete,
        /// <summary>
        /// Used when nodes are published
        /// </summary>
        Publish,
        /// <summary>
        /// Used when nodes are send to publishing
        /// </summary>
        SendToPublish,
        /// <summary>
        /// Used when nodes are unpublished
        /// </summary>
        UnPublish,
        /// <summary>
        /// Used when nodes are moved
        /// </summary>
        Move,
        /// <summary>
        /// Used when nodes are copied
        /// </summary>
        Copy,
        /// <summary>
        /// Used when nodes are assígned a domain
        /// </summary>
        AssignDomain,
        /// <summary>
        /// Used when public access are changed for a node
        /// </summary>
        PublicAccess,
        /// <summary>
        /// Used when nodes are sorted
        /// </summary>
        Sort,
        /// <summary>
        /// Used when a notification are send to a user
        /// </summary>
        Notify,
        /// <summary>
        /// General system notification
        /// </summary>
        System,
        /// <summary>
        /// Used when a node's content is rolled back to a previous version
        /// </summary>
        RollBack,
        /// <summary>
        /// Used when a package is installed
        /// </summary>
        PackagerInstall,
        /// <summary>
        /// Used when a package is uninstalled
        /// </summary>
        PackagerUninstall,
        /// <summary>
        /// Used when a node is send to translation
        /// </summary>
        SendToTranslate,
        /// <summary>
        /// Use this log action for custom log messages that should be shown in the audit trail
        /// </summary>
        Custom
    }
}
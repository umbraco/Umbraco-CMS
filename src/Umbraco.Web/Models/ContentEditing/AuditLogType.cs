using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Defines audit trail log types
    /// </summary>
    public enum AuditLogType
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
        SendToTranslate

    }
}

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDocumentRepository : IPublishableContentRepository<IContent>
{
    /// <summary>
    ///     Used to bulk update the permissions set for a content item. This will replace all permissions
    ///     assigned to an entity with a list of user id &amp; permission pairs.
    /// </summary>
    /// <param name="permissionSet"></param>
    void ReplaceContentPermissions(EntityPermissionSet permissionSet);

    /// <summary>
    ///     Assigns a single permission to the current content item for the specified user group ids
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="permission"></param>
    /// <param name="groupIds"></param>
    void AssignEntityPermission(IContent entity, string permission, IEnumerable<int> groupIds);

    /// <summary>
    ///     Gets the explicit list of permissions for the content item
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    EntityPermissionCollection GetPermissionsForEntity(int entityId);

    /// <summary>
    ///     Used to add/update a permission for a content item
    /// </summary>
    /// <param name="permission"></param>
    void AddOrUpdatePermissions(ContentPermissionSet permission);

    /// <summary>
    ///     Returns true if there is any content in the recycle bin
    /// </summary>
    bool RecycleBinSmells();

    bool IsPathPublished(IContent? content);

    /// <summary>
    ///     Gets the content keys from the provided collection of keys that are scheduled for publishing.
    /// </summary>
    /// <param name="keys">The content keys.</param>
    /// <returns>
    ///     The provided collection of content keys filtered for those that are scheduled for publishing.
    /// </returns>
    IEnumerable<Guid> GetScheduledContentKeys(Guid[] keys) => [];
}

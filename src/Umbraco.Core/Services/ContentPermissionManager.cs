// src/Umbraco.Core/Services/ContentPermissionManager.cs
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Internal manager for content permission operations.
/// </summary>
/// <remarks>
/// <para>
/// This is an internal class that encapsulates permission operations extracted from ContentService
/// as part of the ContentService refactoring initiative (Phase 6).
/// </para>
/// <para>
/// <strong>Design Decision:</strong> This class is internal (not public interface) because:
/// <list type="bullet">
///   <item><description>Permission operations are tightly coupled to content entities</description></item>
///   <item><description>They don't require independent testability beyond ContentService tests</description></item>
///   <item><description>The public API remains through IContentService for backward compatibility</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Note:</strong> GetPermissionsForEntity returns EntityPermissionCollection which is a
/// materialized collection (not deferred), so scope disposal before enumeration is safe.
/// </para>
/// </remarks>
internal sealed class ContentPermissionManager
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<ContentPermissionManager> _logger;

    public ContentPermissionManager(
        ICoreScopeProvider scopeProvider,
        IDocumentRepository documentRepository,
        ILoggerFactory loggerFactory)
    {
        // v1.1: Use ArgumentNullException.ThrowIfNull for consistency with codebase patterns
        ArgumentNullException.ThrowIfNull(scopeProvider);
        ArgumentNullException.ThrowIfNull(documentRepository);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _scopeProvider = scopeProvider;
        _documentRepository = documentRepository;
        _logger = loggerFactory.CreateLogger<ContentPermissionManager>();
    }

    /// <summary>
    /// Used to bulk update the permissions set for a content item. This will replace all permissions
    /// assigned to an entity with a list of user id &amp; permission pairs.
    /// </summary>
    /// <param name="permissionSet">The permission set to assign.</param>
    public void SetPermissions(EntityPermissionSet permissionSet)
    {
        // v1.1: Add input validation
        ArgumentNullException.ThrowIfNull(permissionSet);

        // v1.1: Add logging for security-relevant operations
        _logger.LogDebug("Replacing all permissions for entity {EntityId}", permissionSet.EntityId);

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);
        _documentRepository.ReplaceContentPermissions(permissionSet);
        scope.Complete();
    }

    /// <summary>
    /// Assigns a single permission to the current content item for the specified group ids.
    /// </summary>
    /// <param name="entity">The content entity.</param>
    /// <param name="permission">The permission character (e.g., "F" for Browse, "U" for Update).</param>
    /// <param name="groupIds">The user group IDs to assign the permission to.</param>
    public void SetPermission(IContent entity, string permission, IEnumerable<int> groupIds)
    {
        // v1.1: Add input validation
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        ArgumentNullException.ThrowIfNull(groupIds);

        // v1.2: Add warning for non-standard permission codes (Umbraco uses single characters)
        if (permission.Length != 1)
        {
            _logger.LogWarning(
                "Permission code {Permission} has length {Length}; expected single character for entity {EntityId}",
                permission, permission.Length, entity.Id);
        }

        // v1.1: Add logging for security-relevant operations
        _logger.LogDebug("Assigning permission {Permission} to groups for entity {EntityId}",
            permission, entity.Id);

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);
        _documentRepository.AssignEntityPermission(entity, permission, groupIds);
        scope.Complete();
    }

    /// <summary>
    /// Returns implicit/inherited permissions assigned to the content item for all user groups.
    /// </summary>
    /// <param name="content">The content item to get permissions for.</param>
    /// <returns>Collection of entity permissions (materialized, not deferred).</returns>
    public EntityPermissionCollection GetPermissions(IContent content)
    {
        // v1.1: Add input validation
        ArgumentNullException.ThrowIfNull(content);

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return _documentRepository.GetPermissionsForEntity(content.Id);
    }
}

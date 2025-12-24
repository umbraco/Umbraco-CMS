// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Manager for content blueprint (template) operations.
/// </summary>
/// <remarks>
/// <para>
/// This class encapsulates blueprint operations extracted from ContentService
/// as part of the ContentService refactoring initiative (Phase 7).
/// </para>
/// <para>
/// <strong>Design Decision:</strong> This class is public for DI but not intended for direct external use:
/// <list type="bullet">
///   <item><description>Blueprint operations are tightly coupled to content entities</description></item>
///   <item><description>They don't require independent testability beyond ContentService tests</description></item>
///   <item><description>The public API remains through IContentService for backward compatibility</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Notifications:</strong> Blueprint operations fire the following notifications:
/// <list type="bullet">
///   <item><description><see cref="ContentSavedBlueprintNotification"/> - after saving a blueprint</description></item>
///   <item><description><see cref="ContentDeletedBlueprintNotification"/> - after deleting blueprint(s)</description></item>
///   <item><description><see cref="ContentTreeChangeNotification"/> - after save/delete for cache invalidation</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ContentBlueprintManager
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDocumentBlueprintRepository _documentBlueprintRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IAuditService _auditService;
    private readonly ILogger<ContentBlueprintManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentBlueprintManager"/> class.
    /// </summary>
    public ContentBlueprintManager(
        ICoreScopeProvider scopeProvider,
        IDocumentBlueprintRepository documentBlueprintRepository,
        ILanguageRepository languageRepository,
        IContentTypeRepository contentTypeRepository,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeProvider);
        ArgumentNullException.ThrowIfNull(documentBlueprintRepository);
        ArgumentNullException.ThrowIfNull(languageRepository);
        ArgumentNullException.ThrowIfNull(contentTypeRepository);
        ArgumentNullException.ThrowIfNull(eventMessagesFactory);
        ArgumentNullException.ThrowIfNull(auditService);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _scopeProvider = scopeProvider;
        _documentBlueprintRepository = documentBlueprintRepository;
        _languageRepository = languageRepository;
        _contentTypeRepository = contentTypeRepository;
        _eventMessagesFactory = eventMessagesFactory;
        _auditService = auditService;
        _logger = loggerFactory.CreateLogger<ContentBlueprintManager>();
    }

    private static readonly string?[] ArrayOfOneNullString = { null };

    /// <summary>
    /// Gets a blueprint by its integer ID.
    /// </summary>
    /// <param name="id">The blueprint ID.</param>
    /// <returns>The blueprint content, or null if not found.</returns>
    public IContent? GetBlueprintById(int id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);

        IContent? blueprint = _documentBlueprintRepository.Get(id);
        if (blueprint is null)
        {
            return null;
        }

        blueprint.Blueprint = true;
        return blueprint;
    }

    /// <summary>
    /// Gets a blueprint by its GUID key.
    /// </summary>
    /// <param name="id">The blueprint GUID key.</param>
    /// <returns>The blueprint content, or null if not found.</returns>
    public IContent? GetBlueprintById(Guid id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);

        IContent? blueprint = _documentBlueprintRepository.Get(id);
        if (blueprint is null)
        {
            return null;
        }

        blueprint.Blueprint = true;
        return blueprint;
    }

    /// <summary>
    /// Saves a blueprint.
    /// </summary>
    /// <param name="content">The blueprint content to save.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    [Obsolete("Use SaveBlueprint(IContent, IContent?, int) instead. Scheduled for removal in V19.")]
    public void SaveBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
        => SaveBlueprint(content, null, userId);

    /// <summary>
    /// Saves a blueprint with optional source content reference.
    /// </summary>
    /// <param name="content">The blueprint content to save.</param>
    /// <param name="createdFromContent">The source content the blueprint was created from, if any.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    public void SaveBlueprint(IContent content, IContent? createdFromContent, int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(content);

        EventMessages evtMsgs = _eventMessagesFactory.Get();

        content.Blueprint = true;

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        if (content.HasIdentity == false)
        {
            content.CreatorId = userId;
        }

        content.WriterId = userId;

        _documentBlueprintRepository.Save(content);

        _auditService.Add(AuditType.Save, userId, content.Id, UmbracoObjectTypes.DocumentBlueprint.GetName(), $"Saved content template: {content.Name}");

        _logger.LogDebug("Saved blueprint {BlueprintId} ({BlueprintName})", content.Id, content.Name);

        scope.Notifications.Publish(new ContentSavedBlueprintNotification(content, createdFromContent, evtMsgs));
        scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.RefreshNode, evtMsgs));

        scope.Complete();
    }

    /// <summary>
    /// Deletes a blueprint.
    /// </summary>
    /// <param name="content">The blueprint content to delete.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    public void DeleteBlueprint(IContent content, int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(content);

        EventMessages evtMsgs = _eventMessagesFactory.Get();

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);
        _documentBlueprintRepository.Delete(content);

        // Audit deletion for security traceability (v2.0: added per critical review)
        _auditService.Add(AuditType.Delete, userId, content.Id, UmbracoObjectTypes.DocumentBlueprint.GetName(), $"Deleted content template: {content.Name}");

        _logger.LogDebug("Deleted blueprint {BlueprintId} ({BlueprintName})", content.Id, content.Name);

        scope.Notifications.Publish(new ContentDeletedBlueprintNotification(content, evtMsgs));
        scope.Notifications.Publish(new ContentTreeChangeNotification(content, TreeChangeTypes.Remove, evtMsgs));
        scope.Complete();
    }

    /// <summary>
    /// Creates a new content item from a blueprint template.
    /// </summary>
    /// <param name="blueprint">The blueprint to create content from.</param>
    /// <param name="name">The name for the new content.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    /// <returns>A new unsaved content item populated from the blueprint.</returns>
    public IContent CreateContentFromBlueprint(
        IContent blueprint,
        string name,
        int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(blueprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // v2.0: Use single scope for entire method (per critical review - avoids scope overhead)
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IContentType contentType = GetContentTypeInternal(blueprint.ContentType.Alias);
        var content = new Content(name, -1, contentType);
        content.Path = string.Concat(content.ParentId.ToString(), ",", content.Id);

        content.CreatorId = userId;
        content.WriterId = userId;

        IEnumerable<string?> cultures = ArrayOfOneNullString;
        if (blueprint.CultureInfos?.Count > 0)
        {
            cultures = blueprint.CultureInfos.Values.Select(x => x.Culture);
            if (blueprint.CultureInfos.TryGetValue(_languageRepository.GetDefaultIsoCode(), out ContentCultureInfos defaultCulture))
            {
                defaultCulture.Name = name;
            }
        }

        DateTime now = DateTime.UtcNow;
        foreach (var culture in cultures)
        {
            foreach (IProperty property in blueprint.Properties)
            {
                var propertyCulture = property.PropertyType.VariesByCulture() ? culture : null;
                content.SetValue(property.Alias, property.GetValue(propertyCulture), propertyCulture);
            }

            if (!string.IsNullOrEmpty(culture))
            {
                content.SetCultureInfo(culture, blueprint.GetCultureName(culture), now);
            }
        }

        return content;
    }

    /// <summary>
    /// Gets all blueprints for the specified content type IDs.
    /// </summary>
    /// <param name="contentTypeId">The content type IDs to filter by (empty returns all).</param>
    /// <returns>Collection of blueprints.</returns>
    public IEnumerable<IContent> GetBlueprintsForContentTypes(params int[] contentTypeId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        // v3.0: Added read lock to match GetBlueprintById pattern (per critical review)
        scope.ReadLock(Constants.Locks.ContentTree);

        IQuery<IContent> query = _scopeProvider.CreateQuery<IContent>();
        if (contentTypeId.Length > 0)
        {
            // Need to use a List here because the expression tree cannot convert the array when used in Contains.
            List<int> contentTypeIdsAsList = [.. contentTypeId];
            query.Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId));
        }

        // v3.0: Materialize to array to avoid double enumeration bug (per critical review)
        // Calling .Count() on IEnumerable then returning it would cause double database query
        IContent[] blueprints = _documentBlueprintRepository.Get(query).Select(x =>
        {
            x.Blueprint = true;
            return x;
        }).ToArray();

        // v2.0: Added debug logging for consistency with other methods (per critical review)
        _logger.LogDebug("Retrieved {Count} blueprints for content types {ContentTypeIds}",
            blueprints.Length, contentTypeId.Length > 0 ? string.Join(", ", contentTypeId) : "(all)");

        return blueprints;
    }

    /// <summary>
    /// Deletes all blueprints of the specified content types.
    /// </summary>
    /// <param name="contentTypeIds">The content type IDs whose blueprints should be deleted.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    /// <remarks>
    /// <para>
    /// <strong>Known Limitation:</strong> Blueprints are deleted one at a time in a loop.
    /// If there are many blueprints (e.g., 100+), this results in N separate delete operations.
    /// This matches the original ContentService behavior and is acceptable for Phase 7
    /// (behavior preservation). A bulk delete optimization could be added in a future phase
    /// if IDocumentBlueprintRepository is extended with a bulk delete method.
    /// </para>
    /// </remarks>
    public void DeleteBlueprintsOfTypes(IEnumerable<int> contentTypeIds, int userId = Constants.Security.SuperUserId)
    {
        ArgumentNullException.ThrowIfNull(contentTypeIds);

        // v3.0: Guard against accidental deletion of all blueprints (per critical review)
        // An empty array means "delete blueprints of no types" = do nothing (not "delete all")
        var contentTypeIdsAsList = contentTypeIds.ToList();
        if (contentTypeIdsAsList.Count == 0)
        {
            _logger.LogDebug("DeleteBlueprintsOfTypes called with empty contentTypeIds, no action taken");
            return;
        }

        EventMessages evtMsgs = _eventMessagesFactory.Get();

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.ContentTree);

        IQuery<IContent> query = _scopeProvider.CreateQuery<IContent>();
        query.Where(x => contentTypeIdsAsList.Contains(x.ContentTypeId));

        IContent[]? blueprints = _documentBlueprintRepository.Get(query)?.Select(x =>
        {
            x.Blueprint = true;
            return x;
        }).ToArray();

        // v2.0: Early return with scope.Complete() to ensure scope completes in all paths (per critical review)
        if (blueprints is null || blueprints.Length == 0)
        {
            scope.Complete();
            return;
        }

        foreach (IContent blueprint in blueprints)
        {
            _documentBlueprintRepository.Delete(blueprint);
        }

        // v2.0: Added audit logging for security traceability (per critical review)
        _auditService.Add(AuditType.Delete, userId, -1, UmbracoObjectTypes.DocumentBlueprint.GetName(),
            $"Deleted {blueprints.Length} content template(s) for content types: {string.Join(", ", contentTypeIdsAsList)}");

        _logger.LogDebug("Deleted {Count} blueprints for content types {ContentTypeIds}",
            blueprints.Length, string.Join(", ", contentTypeIdsAsList));

        scope.Notifications.Publish(new ContentDeletedBlueprintNotification(blueprints, evtMsgs));
        scope.Notifications.Publish(new ContentTreeChangeNotification(blueprints, TreeChangeTypes.Remove, evtMsgs));
        scope.Complete();
    }

    /// <summary>
    /// Deletes all blueprints of the specified content type.
    /// </summary>
    /// <param name="contentTypeId">The content type ID whose blueprints should be deleted.</param>
    /// <param name="userId">The user ID performing the operation.</param>
    public void DeleteBlueprintsOfType(int contentTypeId, int userId = Constants.Security.SuperUserId) =>
        DeleteBlueprintsOfTypes(new[] { contentTypeId }, userId);

    /// <summary>
    /// Gets the content type by alias, throwing if not found.
    /// </summary>
    /// <remarks>
    /// This is an internal helper that assumes a scope is already active.
    /// </remarks>
    private IContentType GetContentTypeInternal(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        IContentType? contentType = _contentTypeRepository.Get(alias);

        if (contentType == null)
        {
            throw new InvalidOperationException($"Content type with alias '{alias}' not found.");
        }

        return contentType;
    }

    // v3.0: Removed unused GetContentType(string) method (per critical review - dead code)
}

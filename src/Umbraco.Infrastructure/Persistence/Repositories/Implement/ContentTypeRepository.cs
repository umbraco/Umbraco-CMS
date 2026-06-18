using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

// EF1002: the raw SQL below interpolates only compile-time identifier constants (table/column names). All
// caller-supplied values (IQuery clause arguments) are bound through the positional parameter array. Identifiers
// cannot be passed as parameters, so ExecuteSqlInterpolated is not an option; the query is not vulnerable to SQL injection.
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IContentType" /> (document types).
/// </summary>
/// <remarks>
///     The shared content-type-composition logic lives in <see cref="AsyncContentTypeRepositoryBase{TEntity}"/>;
///     this class only supplies the document-type specifics: the node object type, template and history-cleanup
///     persistence, and the document-type-only query methods on <see cref="IContentTypeRepository"/>.
/// </remarks>
internal sealed class ContentTypeRepository : AsyncContentTypeRepositoryBase<IContentType>, IContentTypeRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeRepository"/> class.
    /// </summary>
    public ContentTypeRepository(
        AppCaches cache,
        ILogger<ContentTypeRepository> logger,
        IContentTypeCommonRepository commonRepository,
        ILanguageRepository languageRepository,
        IShortStringHelper shortStringHelper,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        IIdKeyMap idKeyMap,
        ICacheSyncService cacheSyncService,
        IEFCoreScopeAccessor<UmbracoDbContext> efCoreScopeAccessor)
        : base(
            cache,
            logger,
            commonRepository,
            languageRepository,
            shortStringHelper,
            repositoryCacheVersionService,
            idKeyMap,
            cacheSyncService,
            efCoreScopeAccessor)
    {
    }

    /// <inheritdoc />
    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentType;

    /// <inheritdoc />
    protected override bool SupportsPublishing => ContentType.SupportsPublishingConst;

    // ----------------------------------------------------------------------------------------------------
    // Document-type-specific query methods (IContentTypeRepository).
    // ----------------------------------------------------------------------------------------------------

    /// <inheritdoc />
    public IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query)
    {
        var ints = PerformGetByQuery(query).ToArray();
        return ints.Length > 0 ? GetMany(ints) : Enumerable.Empty<IContentType>();
    }

    private IEnumerable<int> PerformGetByQuery(IQuery<PropertyType> query)
    {
        // used by DataTypeService to remove properties from content types if they have a deleted data type.
        // Matches the legacy behaviour of resolving the content type through the property GROUP — ungrouped
        // property types resolve to 0 and are filtered out.
        (var whereSql, var args) = TranslateWhereClauses(query.GetWhereClauses());
        var sql =
            $"""
             SELECT DISTINCT COALESCE({PropertyTypeGroupDto.TableName}.{PropertyTypeGroupDto.ContentTypeNodeIdColumnName}, 0) AS Value
             FROM {PropertyTypeDto.TableName}
             LEFT JOIN {PropertyTypeGroupDto.TableName} ON {PropertyTypeGroupDto.TableName}.{PropertyTypeGroupDto.PrimaryKeyColumnName} = {PropertyTypeDto.TableName}.{PropertyTypeDto.PropertyTypeGroupIdColumnName}
             INNER JOIN {DataTypeDto.TableName} ON {PropertyTypeDto.TableName}.{PropertyTypeDto.DataTypeIdColumnName} = {DataTypeDto.TableName}.nodeId
             WHERE 1 = 1{whereSql}
             """;

        return ExecuteEfScope(db => db.Database.SqlQueryRaw<int>(sql, args).ToList())
            .Where(id => id > 0)
            .Distinct();
    }

    /// <summary>
    ///     Gets all property type aliases.
    /// </summary>
    public IEnumerable<string> GetAllPropertyTypeAliases()
        => ExecuteEfScope(db => db.PropertyTypes
            .Select(x => x.Alias!)
            .Distinct()
            .OrderBy(x => x)
            .ToList());

    /// <summary>
    ///     Gets all content type aliases.
    /// </summary>
    public IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes)
        => ExecuteEfScope(db =>
        {
            IQueryable<ContentTypeDto> query = db.ContentTypes;

            if (objectTypes.Any())
            {
                query = query.Where(x =>
                    x.NodeDto.NodeObjectType.HasValue && objectTypes.Contains(x.NodeDto.NodeObjectType.Value));
            }

            return query.Select(x => x.Alias!).ToList();
        });

    /// <summary>
    ///     Retrieves the IDs of all content types that match the specified aliases.
    /// </summary>
    public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
    {
        if (aliases.Length == 0)
        {
            return Enumerable.Empty<int>();
        }

        return ExecuteEfScope(db => db.ContentTypes
            .Where(x => aliases.Contains(x.Alias))
            .Select(x => x.NodeId)
            .ToList());
    }

    // ----------------------------------------------------------------------------------------------------
    // Document-type-specific persistence (templates, history cleanup) layered on top of the shared base.
    // ----------------------------------------------------------------------------------------------------

    /// <inheritdoc />
    protected override void PersistNewItem(IContentType entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Alias))
        {
            var ex = new Exception(
                $"ContentType '{entity.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");
            Logger.LogError(
                "ContentType '{EntityName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                entity.Name);
            throw ex;
        }

        entity.AddingEntity();

        PersistNewBaseContentType(entity);
        PersistTemplates(entity);
        PersistHistoryCleanup(entity);

        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override void PersistUpdatedItem(IContentType entity)
    {
        ValidateAlias(entity);

        // Updates Modified date
        entity.UpdatingEntity();

        // Look up parent to get and set the correct Path if ParentId has changed
        if (entity.IsPropertyDirty("ParentId"))
        {
            var parent = ExecuteEfScope(db => db.Nodes
                .Where(x => x.NodeId == entity.ParentId)
                .Select(x => new { x.Path, x.Level })
                .First());
            entity.Path = string.Concat(parent.Path, ",", entity.Id);
            entity.Level = parent.Level + 1;

            var maxSortOrder = ExecuteEfScope(db => db.Nodes
                .Where(x => x.ParentId == entity.ParentId && x.NodeObjectType == NodeObjectTypeId)
                .Select(x => (int?)x.SortOrder)
                .Max()) ?? 0;
            entity.SortOrder = maxSortOrder + 1;
        }

        PersistUpdatedBaseContentType(entity);
        PersistTemplates(entity);
        PersistHistoryCleanup(entity);

        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override void DeleteContentTypeSpecificDefinitionTables(UmbracoDbContext db, int id)
    {
        db.ContentTypeTemplates.Where(x => x.ContentTypeNodeId == id).ExecuteDelete();
        db.ContentVersionCleanupPolicies.Where(x => x.ContentTypeId == id).ExecuteDelete();
    }

    private void PersistTemplates(IContentType entity)
    {
        var defaultTemplateId = entity.DefaultTemplateId;
        ITemplate[] allowedTemplates = entity.AllowedTemplates?.Where(x => x.Id != defaultTemplateId).ToArray()
                                       ?? Array.Empty<ITemplate>();

        ExecuteEfScope(db =>
        {
            // remove and re-insert. ExecuteDelete is set-based and bypasses the change tracker.
            db.ContentTypeTemplates.Where(x => x.ContentTypeNodeId == entity.Id).ExecuteDelete();

            if (defaultTemplateId > 0)
            {
                db.ContentTypeTemplates.Add(new ContentTypeTemplateDto
                {
                    ContentTypeNodeId = entity.Id,
                    TemplateNodeId = defaultTemplateId,
                    IsDefault = true,
                });
            }

            foreach (ITemplate template in allowedTemplates)
            {
                db.ContentTypeTemplates.Add(new ContentTypeTemplateDto
                {
                    ContentTypeNodeId = entity.Id,
                    TemplateNodeId = template.Id,
                    IsDefault = false,
                });
            }

            db.SaveChanges();
        });
    }

    private void PersistHistoryCleanup(IContentType entity)
    {
        // historyCleanup property is not mandatory for api endpoint, handle the case where it's not present.
        if (entity is IContentType entityWithHistoryCleanup)
        {
            var updated = DateTime.UtcNow;
            var preventCleanup = entityWithHistoryCleanup.HistoryCleanup?.PreventCleanup ?? false;
            var keepAllVersionsNewerThanDays = entityWithHistoryCleanup.HistoryCleanup?.KeepAllVersionsNewerThanDays;
            var keepLatestVersionPerDayForDays = entityWithHistoryCleanup.HistoryCleanup?.KeepLatestVersionPerDayForDays;

            ExecuteEfScope(db =>
            {
                // Upsert: ExecuteUpdate is set-based (no change-tracker conflict); insert only if absent.
                var affected = db.ContentVersionCleanupPolicies
                    .Where(x => x.ContentTypeId == entity.Id)
                    .ExecuteUpdate(s => s
                        .SetProperty(x => x.Updated, updated)
                        .SetProperty(x => x.PreventCleanup, preventCleanup)
                        .SetProperty(x => x.KeepAllVersionsNewerThanDays, keepAllVersionsNewerThanDays)
                        .SetProperty(x => x.KeepLatestVersionPerDayForDays, keepLatestVersionPerDayForDays));

                if (affected == 0)
                {
                    db.ContentVersionCleanupPolicies.Add(new ContentVersionCleanupPolicyDto
                    {
                        ContentTypeId = entity.Id,
                        Updated = updated,
                        PreventCleanup = preventCleanup,
                        KeepAllVersionsNewerThanDays = keepAllVersionsNewerThanDays,
                        KeepLatestVersionPerDayForDays = keepLatestVersionPerDayForDays,
                    });
                    db.SaveChanges();
                }
            });
        }
    }
}

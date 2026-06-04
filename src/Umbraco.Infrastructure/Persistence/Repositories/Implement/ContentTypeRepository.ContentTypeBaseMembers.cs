using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

// Members absorbed from the former shared base ContentTypeRepositoryBase<TEntity> when ContentTypeRepository
// was moved onto the async EF Core base. MediaType/MemberType keep using the shared NPoco base unchanged.
// All database access here runs through the EF Core UmbracoDbContext — typed LINQ for tables modelled in EF,
// raw SQL (ExecuteSqlRaw/SqlQueryRaw) for content-instance tables owned by other repositories (their names
// come from Constants.DatabaseSchema, with column names inlined). Raw SQL conventions: server-generated
// integer ids are inlined (avoids the SQL Server 2100-parameter limit); reserved identifiers ("group",
// "current", "text") are double-quoted, which both SQL Server (QUOTED_IDENTIFIER ON) and SQLite accept;
// Guid values are bound as parameters, never string literals (provider-specific storage formats).
internal sealed partial class ContentTypeRepository
{
    /// <summary>
    /// Moves the specified content type entity to a new container or to the root if the container is <c>null</c>.
    /// Updates the entity's parent, path, and level, and also updates all descendant entities accordingly.
    /// </summary>
    public IEnumerable<MoveEventInfo<IContentType>> Move(IContentType moving, EntityContainer? container)
    {
        var parentId = Constants.System.Root;
        Guid? parentKey = Constants.System.RootKey;
        if (container != null)
        {
            // check path
            if (string.Format(",{0},", container.Path).IndexOf(
                string.Format(",{0},", moving.Id),
                StringComparison.Ordinal) > -1)
            {
                throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType
                    .FailedNotAllowedByPath);
            }

            parentId = container.Id;
            parentKey = container.Key;
        }

        // track moved entities
        var moveInfo = new List<MoveEventInfo<IContentType>> { new(moving, moving.Path, parentKey) };

        // get the level delta (old pos to new pos)
        var levelDelta = container == null
            ? 1 - moving.Level
            : container.Level + 1 - moving.Level;

        // move to parent (or -1), update path, save
        moving.ParentId = parentId;
        var movingPath = moving.Path + ","; // save before changing
        moving.Path = (container == null ? Constants.System.RootString : container.Path) + "," + moving.Id;
        moving.Level = container == null ? 1 : container.Level + 1;
        Save(moving);

        // update all descendants (from the cached full set), update in order of level
        IContentType[] descendants = GetAllCached()
            .Where(type => type.Path.StartsWith(movingPath, StringComparison.Ordinal))
            .ToArray();
        var paths = new Dictionary<int, string>
        {
            [moving.Id] = moving.Path,
        };

        foreach (IContentType descendant in descendants.OrderBy(x => x.Level))
        {
            moveInfo.Add(new MoveEventInfo<IContentType>(descendant, descendant.Path, descendant.ParentId));

            descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
            descendant.Level += levelDelta;

            Save(descendant);
        }

        return moveInfo;
    }

    private PropertyType CreatePropertyType(
        string propertyEditorAlias,
        ValueStorageType storageType,
        string propertyTypeAlias) =>
        new PropertyType(_shortStringHelper, propertyEditorAlias, storageType, propertyTypeAlias);

    /// <summary>
    /// When content types are removed from a composition (U4-1690), clears the orphaned property data
    /// on content of <paramref name="entity"/> for property types that belonged to the removed types.
    /// </summary>
    private void ClearPropertyDataForRemovedContentTypes(IContentTypeComposition entity)
    {
        if (!entity.RemovedContentTypes.Any())
        {
            return;
        }

        ExecuteEfScope(db =>
        {
            // note: Guid values must be bound as parameters (not string literals) — providers store/bind them
            // in provider-specific formats (e.g. uppercase TEXT on SQLite, uniqueidentifier on SQL Server).
            var p0 = "{0}";
            foreach (var key in entity.RemovedContentTypes)
            {
                // delete property data on content of this content type, for property types belonging to
                // the removed composition type
                db.Database.ExecuteSqlRaw(
                    $"""
                     DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE id IN (
                     SELECT pd.id
                     FROM {Constants.DatabaseSchema.Tables.PropertyData} pd
                     INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON pd.versionId = cv.id
                     INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON cv.nodeId = c.nodeId
                     INNER JOIN {NodeDto.TableName} n ON c.nodeId = n.{NodeDto.PrimaryKeyColumnName}
                     INNER JOIN {PropertyTypeDto.TableName} pt ON pd.propertyTypeId = pt.{PropertyTypeDto.PrimaryKeyColumnName}
                     WHERE n.{NodeDto.NodeObjectTypeColumnName} = {p0} AND c.contentTypeId = {entity.Id} AND pt.{PropertyTypeDto.ContentTypeIdColumnName} = {key})
                     """,
                    Constants.ObjectTypes.Document);
            }
        });
    }

    private void ValidateAlias(IPropertyType pt)
    {
        if (string.IsNullOrWhiteSpace(pt.Alias))
        {
            var ex = new InvalidOperationException(
                $"Property Type '{pt.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");

            Logger.LogError(
                "Property Type '{PropertyTypeName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                pt.Name);

            throw ex;
        }
    }

    private void ValidateAlias(IContentType entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Alias))
        {
            var ex = new InvalidOperationException(
                $"{nameof(IContentType)} '{entity.Name}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.");

            Logger.LogError(
                "{EntityTypeName} '{EntityName}' cannot have an empty Alias. This is most likely due to invalid characters stripped from the Alias.",
                nameof(IContentType),
                entity.Name);

            throw ex;
        }
    }

    private static bool IsPropertyValueChanged(PropertyValueVersionDto pubRow, PropertyValueVersionDto row) =>
        (!pubRow.TextValue.IsNullOrWhiteSpace() && pubRow.TextValue != row.TextValue)
        || (!pubRow.VarcharValue.IsNullOrWhiteSpace() && pubRow.VarcharValue != row.VarcharValue)
        || (pubRow.DateValue.HasValue && pubRow.DateValue != row.DateValue)
        || (pubRow.DecimalValue.HasValue && pubRow.DecimalValue != row.DecimalValue)
        || (pubRow.IntValue.HasValue && pubRow.IntValue != row.IntValue);

    /// <summary>
    ///     Corrects the property type variations for the given entity to make sure the property type variation is
    ///     compatible with the variation set on the entity itself.
    /// </summary>
    private void CorrectPropertyTypeVariations(IContentTypeComposition entity)
    {
        foreach (IPropertyType propertyType in entity.PropertyTypes)
        {
            propertyType.Variations = entity.Variations & propertyType.Variations;
        }
    }

    /// <summary>
    ///     Ensures that no property types are flagged for a variance that is not supported by the content type itself.
    /// </summary>
    private void ValidateVariations(IContentTypeComposition entity)
    {
        foreach (IPropertyType prop in entity.PropertyTypes)
        {
            var isValid = entity.Variations.HasFlag(prop.Variations);
            if (!isValid)
            {
                throw new InvalidOperationException(
                    $"The property {prop.Alias} cannot have variations of {prop.Variations} with the content type variations of {entity.Variations}");
            }
        }
    }

    private IEnumerable<IContentTypeComposition> GetImpactedContentTypes(
        IContentTypeComposition contentType,
        IEnumerable<IContentTypeComposition>? all)
    {
        if (all is null)
        {
            return Enumerable.Empty<IContentTypeComposition>();
        }

        var impact = new List<IContentTypeComposition>();
        var set = new List<IContentTypeComposition> { contentType };

        var tree = new Dictionary<int, List<IContentTypeComposition>>();
        foreach (IContentTypeComposition x in all)
        {
            foreach (IContentTypeComposition y in x.ContentTypeComposition)
            {
                if (!tree.TryGetValue(y.Id, out List<IContentTypeComposition>? list))
                {
                    list = tree[y.Id] = new List<IContentTypeComposition>();
                }

                list.Add(x);
            }
        }

        var nset = new List<IContentTypeComposition>();
        do
        {
            impact.AddRange(set);

            foreach (IContentTypeComposition x in set)
            {
                if (!tree.TryGetValue(x.Id, out List<IContentTypeComposition>? list))
                {
                    continue;
                }

                nset.AddRange(list.Where(y => y.VariesByCulture()));
            }

            set = nset;
            nset = new List<IContentTypeComposition>();
        }
        while (set.Count > 0);

        return impact;
    }

    // gets property types that have actually changed, and the corresponding changes
    // returns null if no property type has actually changed
    private Dictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)>?
        GetPropertyVariationChanges(IEnumerable<IPropertyType> propertyTypes)
    {
        var propertyTypesL = propertyTypes.ToList();
        var propertyTypeIds = propertyTypesL.Select(x => x.Id).ToList();

        // select the current variations (before the change) from database
        Dictionary<int, byte> oldVariations = ExecuteEfScope(db => db.PropertyTypes
            .Where(x => propertyTypeIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Variations })
            .ToDictionary(x => x.Id, x => x.Variations));

        Dictionary<int, (ContentVariation, ContentVariation)>? changes = null;

        foreach (IPropertyType propertyType in propertyTypesL)
        {
            if (!oldVariations.TryGetValue(propertyType.Id, out var oldVariationB))
            {
                continue;
            }

            var oldVariation = (ContentVariation)oldVariationB;

            ContentVariation newVariation = propertyType.Variations;
            if (oldVariation == newVariation)
            {
                continue;
            }

            changes ??= new Dictionary<int, (ContentVariation, ContentVariation)>();
            changes[propertyType.Id] = (oldVariation, newVariation);
        }

        return changes;
    }

    /// <summary>
    ///     Clear any redirects associated with content for a content type.
    /// </summary>
    private void Clear301Redirects(IContentTypeComposition contentType)
        => ExecuteEfScope(db => db.Database.ExecuteSqlRaw(
            $"""
             DELETE FROM {Constants.DatabaseSchema.Tables.RedirectUrl} WHERE contentKey IN (
             SELECT n.{NodeDto.KeyColumnName}
             FROM {NodeDto.TableName} n
             INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = n.{NodeDto.PrimaryKeyColumnName}
             WHERE c.contentTypeId = {contentType.Id})
             """));

    /// <summary>
    ///     Clear any scheduled publishing associated with content for a content type.
    /// </summary>
    private void ClearScheduledPublishing(IContentTypeComposition contentType)
    {
        // TODO: Fill this in when scheduled publishing is enabled for variants
    }

    private int GetDefaultLanguageId()
        => ExecuteEfScope(db => db.Language.Where(x => x.IsDefault).Select(x => x.Id).First());

    /// <summary>
    ///     Moves variant data for property type variation changes.
    /// </summary>
    private void MovePropertyTypeVariantData(
        IDictionary<int, (ContentVariation FromVariation, ContentVariation ToVariation)> propertyTypeChanges,
        IEnumerable<IContentTypeComposition> impacted)
    {
        var defaultLanguageId = GetDefaultLanguageId();
        var impactedL = impacted.Select(x => x.Id).ToList();

        foreach (IGrouping<(ContentVariation FromVariation, ContentVariation ToVariation),
                     KeyValuePair<int, (ContentVariation FromVariation, ContentVariation ToVariation)>> grouping in
                 propertyTypeChanges.GroupBy(x => x.Value))
        {
            var propertyTypeIds = grouping.Select(x => x.Key).ToList();
            (ContentVariation fromVariation, ContentVariation toVariation) = grouping.Key;

            var fromCultureEnabled = fromVariation.HasFlag(ContentVariation.Culture);
            var toCultureEnabled = toVariation.HasFlag(ContentVariation.Culture);

            if (!fromCultureEnabled && toCultureEnabled)
            {
                // Culture has been enabled
                CopyPropertyData(null, defaultLanguageId, propertyTypeIds, impactedL);
                CopyTagData(null, defaultLanguageId, propertyTypeIds, impactedL);
                RenormalizeDocumentEditedFlags(propertyTypeIds, impactedL);
            }
            else if (fromCultureEnabled && !toCultureEnabled)
            {
                // Culture has been disabled
                CopyPropertyData(defaultLanguageId, null, propertyTypeIds, impactedL);
                CopyTagData(defaultLanguageId, null, propertyTypeIds, impactedL);
                RenormalizeDocumentEditedFlags(propertyTypeIds, impactedL);
            }
        }
    }

    /// <summary>
    ///     Moves variant data for a content type variation change.
    /// </summary>
    private void MoveContentTypeVariantData(
        IContentTypeComposition contentType,
        ContentVariation fromVariation,
        ContentVariation toVariation)
    {
        var defaultLanguageId = GetDefaultLanguageId();

        var cultureIsNotEnabled = !fromVariation.HasFlag(ContentVariation.Culture);
        var cultureWillBeEnabled = toVariation.HasFlag(ContentVariation.Culture);

        if (!cultureIsNotEnabled || !cultureWillBeEnabled)
        {
            return;
        }

        // move the names: first clear out any existing names that might already exist under the default lang,
        // then insert names into the two culture-variation tables based on the invariant data
        ExecuteEfScope(db =>
        {
            // clear out the versionCultureVariation table
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.ContentVersionCultureVariation} WHERE id IN (
                 SELECT ccv.id
                 FROM {Constants.DatabaseSchema.Tables.ContentVersionCultureVariation} ccv
                 INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON cv.id = ccv.versionId
                 INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = cv.nodeId
                 WHERE c.contentTypeId = {contentType.Id} AND ccv.languageId = {defaultLanguageId})
                 """);

            // clear out the documentCultureVariation table
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.DocumentCultureVariation} WHERE id IN (
                 SELECT dcv.id
                 FROM {Constants.DatabaseSchema.Tables.DocumentCultureVariation} dcv
                 INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = dcv.nodeId
                 WHERE c.contentTypeId = {contentType.Id} AND dcv.languageId = {defaultLanguageId})
                 """);

            // insert rows into the versionCultureVariation table based on contentVersion data for the default lang
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.ContentVersionCultureVariation} (versionId, "name", availableUserId, "date", languageId)
                 SELECT cv.id, cv."text", cv.userId, cv.versionDate, {defaultLanguageId}
                 FROM {Constants.DatabaseSchema.Tables.ContentVersion} cv
                 INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = cv.nodeId
                 WHERE c.contentTypeId = {contentType.Id}
                 """);

            // insert rows into the documentCultureVariation table (make Available + default language ID)
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.DocumentCultureVariation} (nodeId, edited, published, "name", available, languageId)
                 SELECT d.nodeId, d.edited, d.published, n."text", 1, {defaultLanguageId}
                 FROM {Constants.DatabaseSchema.Tables.Document} d
                 INNER JOIN {NodeDto.TableName} n ON n.{NodeDto.PrimaryKeyColumnName} = d.nodeId
                 INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = n.{NodeDto.PrimaryKeyColumnName}
                 WHERE c.contentTypeId = {contentType.Id}
                 """);
        });
    }

    private void CopyTagData(
        int? sourceLanguageId,
        int? targetLanguageId,
        IReadOnlyCollection<int> propertyTypeIds,
        IReadOnlyCollection<int>? contentTypeIds = null)
    {
        if (propertyTypeIds.Count == 0)
        {
            return;
        }

        var pts = string.Join(",", propertyTypeIds);
        var cts = contentTypeIds is { Count: > 0 } ? string.Join(",", contentTypeIds) : null;
        var contentJoin = cts is null ? string.Empty : $"INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON r.nodeId = c.nodeId";
        var contentWhere = cts is null ? string.Empty : $"AND c.contentTypeId IN ({cts})";

        // note: nullable language ids are compared via COALESCE(x, -1), mirroring NPoco's SqlNullableEquals
        var srcCmp = sourceLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "-1";
        var targetCmp = targetLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "-1";
        var targetLiteral = targetLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "NULL";

        ExecuteEfScope(db =>
        {
            // delete existing relations (for target language); do *not* delete existing tags
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.TagRelationship} WHERE tagId IN (
                 SELECT t.id
                 FROM {Constants.DatabaseSchema.Tables.Tag} t
                 INNER JOIN {Constants.DatabaseSchema.Tables.TagRelationship} r ON t.id = r.tagId
                 {contentJoin}
                 WHERE r.propertyTypeId IN ({pts})
                 {contentWhere}
                 AND COALESCE(t.languageId, -1) = {targetCmp})
                 """);

            // copy tags from source language to target language; target tags may exist already, so check
            // for existence via the "xtags" left join
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.Tag} (tag, "group", languageId)
                 SELECT DISTINCT t.tag, t."group", {targetLiteral}
                 FROM {Constants.DatabaseSchema.Tables.Tag} t
                 INNER JOIN {Constants.DatabaseSchema.Tables.TagRelationship} r ON t.id = r.tagId
                 LEFT JOIN {Constants.DatabaseSchema.Tables.Tag} xtags ON t.tag = xtags.tag AND t."group" = xtags."group" AND COALESCE(xtags.languageId, -1) = {targetCmp}
                 {contentJoin}
                 WHERE r.propertyTypeId IN ({pts})
                 {contentWhere}
                 AND xtags.id IS NULL
                 AND COALESCE(t.languageId, -1) = {srcCmp}
                 """);

            // create relations to the new tags (existing target relations were deleted above)
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.TagRelationship} (nodeId, propertyTypeId, tagId)
                 SELECT DISTINCT r.nodeId, r.propertyTypeId, otag.id
                 FROM {Constants.DatabaseSchema.Tables.TagRelationship} r
                 INNER JOIN {Constants.DatabaseSchema.Tables.Tag} t ON r.tagId = t.id
                 INNER JOIN {Constants.DatabaseSchema.Tables.Tag} otag ON t.tag = otag.tag AND t."group" = otag."group" AND COALESCE(otag.languageId, -1) = {targetCmp}
                 {contentJoin}
                 WHERE COALESCE(t.languageId, -1) = {srcCmp}
                 AND r.propertyTypeId IN ({pts})
                 {contentWhere}
                 """);

            // delete original relations - *not* the tags - all of them
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.TagRelationship} WHERE tagId IN (
                 SELECT t.id
                 FROM {Constants.DatabaseSchema.Tables.Tag} t
                 INNER JOIN {Constants.DatabaseSchema.Tables.TagRelationship} r ON t.id = r.tagId
                 {contentJoin}
                 WHERE r.propertyTypeId IN ({pts})
                 {contentWhere}
                 AND COALESCE(t.languageId, -1) <> {targetCmp})
                 """);
        });
    }

    /// <summary>
    ///     Copies property data from one language to another.
    /// </summary>
    /// <param name="sourceLanguageId">The source language (can be null ie invariant).</param>
    /// <param name="targetLanguageId">The target language (can be null ie invariant)</param>
    /// <param name="propertyTypeIds">The property type identifiers.</param>
    /// <param name="contentTypeIds">The content type identifiers.</param>
    private void CopyPropertyData(
        int? sourceLanguageId,
        int? targetLanguageId,
        IReadOnlyCollection<int> propertyTypeIds,
        IReadOnlyCollection<int>? contentTypeIds = null)
    {
        if (propertyTypeIds.Count == 0)
        {
            return;
        }

        var pts = string.Join(",", propertyTypeIds);
        var cts = contentTypeIds is { Count: > 0 } ? string.Join(",", contentTypeIds) : null;
        var versionScope = cts is null
            ? string.Empty
            : $"""
               versionId IN (
               SELECT cv.id
               FROM {Constants.DatabaseSchema.Tables.ContentVersion} cv
               INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON cv.nodeId = c.nodeId
               WHERE c.contentTypeId IN ({cts})) AND
               """;

        var srcCmp = sourceLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "-1";
        var targetCmp = targetLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "-1";
        var targetLiteral = targetLanguageId?.ToString(CultureInfo.InvariantCulture) ?? "NULL";

        ExecuteEfScope(db =>
        {
            // first clear out any existing property data that might already exist under the target language
            db.Database.ExecuteSqlRaw(
                $"""
                 DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE
                 {versionScope}
                 COALESCE(languageId, -1) = {targetCmp}
                 AND propertyTypeId IN ({pts})
                 """);

            // now insert all property data into the target language that exists under the source language
            var sourceJoin = cts is null
                ? string.Empty
                : $"""
                   INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON pd.versionId = cv.id
                   INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON cv.nodeId = c.nodeId
                   """;
            var sourceWhere = cts is null ? string.Empty : $"AND c.contentTypeId IN ({cts})";
            db.Database.ExecuteSqlRaw(
                $"""
                 INSERT INTO {Constants.DatabaseSchema.Tables.PropertyData} (versionId, propertyTypeId, segment, intValue, decimalValue, dateValue, varcharValue, textValue, languageId)
                 SELECT pd.versionId, pd.propertyTypeId, pd.segment, pd.intValue, pd.decimalValue, pd.dateValue, pd.varcharValue, pd.textValue, {targetLiteral}
                 FROM {Constants.DatabaseSchema.Tables.PropertyData} pd
                 {sourceJoin}
                 WHERE COALESCE(pd.languageId, -1) = {srcCmp}
                 AND pd.propertyTypeId IN ({pts})
                 {sourceWhere}
                 """);

            // when copying from Culture, keep the original values around in case we want to go back
            // when copying from Nothing, kill the original values, we don't want them around
            if (sourceLanguageId == null)
            {
                db.Database.ExecuteSqlRaw(
                    $"""
                     DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE
                     {versionScope}
                     languageId IS NULL
                     AND propertyTypeId IN ({pts})
                     """);
            }
        });
    }

    /// <summary>
    ///     Re-normalizes the edited value in the umbracoDocumentCultureVariation and umbracoDocument table when
    ///     variations are changed.
    /// </summary>
    private void RenormalizeDocumentEditedFlags(
        IReadOnlyCollection<int> propertyTypeIds,
        IReadOnlyCollection<int>? contentTypeIds = null)
    {
        if (propertyTypeIds.Count == 0)
        {
            return;
        }

        // TODO: Await this properly when the repository goes fully async.
        var defaultLang = LanguageRepository.GetDefaultIdAsync().GetAwaiter().GetResult();

        var pts = string.Join(",", propertyTypeIds);
        var cts = contentTypeIds is { Count: > 0 } ? string.Join(",", contentTypeIds) : null;
        var contentJoin = cts is null ? string.Empty : $"INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON c.nodeId = cv.nodeId";
        var contentWhere = cts is null ? string.Empty : $"AND c.contentTypeId IN ({cts})";

        // Build a query for the property values of both the current and the published version so we can check,
        // based on the current variance of each item, whether its 'edited' value should be true/false.
        // Note: no ORDER BY in the raw SQL (EF composes raw queries into subqueries); rows are sorted in memory.
        var propertySql =
            $"""
             SELECT pd.versionId AS VersionId, pd.propertyTypeId AS PropertyTypeId,
             pd.languageId AS LanguageId, pd.segment AS Segment, pd.intValue AS IntValue, pd.decimalValue AS DecimalValue,
             pd.dateValue AS DateValue, pd.varcharValue AS VarcharValue, pd.textValue AS TextValue,
             cv.nodeId AS NodeId, cv."current" AS "Current", COALESCE(dv.published, 0) AS Published, pt.variations AS Variations
             FROM {Constants.DatabaseSchema.Tables.PropertyData} pd
             INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON cv.id = pd.versionId
             INNER JOIN {PropertyTypeDto.TableName} pt ON pt.{PropertyTypeDto.PrimaryKeyColumnName} = pd.propertyTypeId
             {contentJoin}
             LEFT JOIN {Constants.DatabaseSchema.Tables.DocumentVersion} dv ON cv.id = dv.id
             WHERE (cv."current" = 1 OR dv.published = 1)
             AND pd.propertyTypeId IN ({pts})
             {contentWhere}
             """;

        List<PropertyValueVersionDto> rows = ExecuteEfScope(db =>
            db.Database.SqlQueryRaw<PropertyValueVersionDto>(propertySql).ToList());

        // Published data must come before Current data, per (nodeId, propertyTypeId, languageId, versionId).
        rows = rows
            .OrderBy(x => x.NodeId)
            .ThenBy(x => x.PropertyTypeId)
            .ThenBy(x => x.LanguageId)
            .ThenBy(x => x.VersionId)
            .ToList();

        // keep track of this node/lang to mark or unmark a culture as edited
        var editedLanguageVersions = new Dictionary<(int nodeId, int? langId), bool>();

        // keep track of which node to mark or unmark as edited
        var editedDocument = new Dictionary<int, bool>();
        var nodeId = -1;
        var propertyTypeId = -1;

        PropertyValueVersionDto? pubRow = null;

        foreach (PropertyValueVersionDto row in rows)
        {
            // make sure to reset on each node/property change
            if (nodeId != row.NodeId || propertyTypeId != row.PropertyTypeId)
            {
                nodeId = row.NodeId;
                propertyTypeId = row.PropertyTypeId;
                pubRow = null;
            }

            if (row.Published)
            {
                pubRow = row;
            }

            if (row.Current)
            {
                var propVariations = (ContentVariation)row.Variations;

                // if this prop doesn't vary but the row has a lang assigned or vice versa, flag this as not edited
                if ((!propVariations.VariesByCulture() && row.LanguageId.HasValue)
                    || (propVariations.VariesByCulture() && !row.LanguageId.HasValue))
                {
                    if (!editedLanguageVersions.TryGetValue((row.NodeId, row.LanguageId), out _))
                    {
                        editedLanguageVersions.Add((row.NodeId, row.LanguageId), false);
                    }

                    editedDocument[row.NodeId] = editedDocument.TryGetValue(row.NodeId, out var edited)
                        ? edited |= false
                        : false;
                }
                else if (pubRow == null)
                {
                    // this property is 'edited' since there is no published version
                    editedLanguageVersions[(row.NodeId, row.LanguageId)] = true;
                    editedDocument[row.NodeId] = true;
                }
                else if (IsPropertyValueChanged(pubRow, row))
                {
                    // an invariant property's edited language is indicated by the default lang
                    editedLanguageVersions[
                        (row.NodeId, !propVariations.VariesByCulture() ? defaultLang : row.LanguageId)] = true;
                    editedDocument[row.NodeId] = true;
                }

                pubRow = null;
            }
        }

        // lookup all matching rows in umbracoDocumentCultureVariation
        // fetch in batches to keep statement size bounded
        var languageIds = editedLanguageVersions.Keys
            .Select(x => x.langId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();
        IEnumerable<int> nodeIds = editedLanguageVersions.Keys.Select(x => x.nodeId).Distinct();
        Dictionary<(int NodeId, int? LanguageId), DocumentCultureVariationRow> docCultureVariationsToUpdate =
            languageIds.Length == 0
                ? new Dictionary<(int, int?), DocumentCultureVariationRow>()
                : nodeIds.InGroupsOf(Constants.Sql.MaxParameterCount - languageIds.Length)
                    .SelectMany(group =>
                    {
                        var sql =
                            $"""
                             SELECT id AS Id, nodeId AS NodeId, languageId AS LanguageId, edited AS Edited
                             FROM {Constants.DatabaseSchema.Tables.DocumentCultureVariation}
                             WHERE languageId IN ({string.Join(",", languageIds)}) AND nodeId IN ({string.Join(",", group)})
                             """;
                        return ExecuteEfScope(db => db.Database.SqlQueryRaw<DocumentCultureVariationRow>(sql).ToList());
                    })
                    .ToDictionary(
                        x => (x.NodeId, (int?)x.LanguageId),
                        x => x);

        var toUpdate = new List<DocumentCultureVariationRow>();
        foreach (KeyValuePair<(int nodeId, int? langId), bool> ev in editedLanguageVersions)
        {
            if (docCultureVariationsToUpdate.TryGetValue(ev.Key, out DocumentCultureVariationRow? docVariations))
            {
                if (docVariations.Edited != ev.Value)
                {
                    docVariations.Edited = ev.Value;
                    toUpdate.Add(docVariations);
                }
            }
            else if (ev.Key.langId.HasValue)
            {
                // This can happen when a property changes from invariant to variant and the content was only
                // created in non-default languages: there is no DocumentCultureVariation row for the default
                // language, so there is no edited flag to update.
                continue;
            }
        }

        ExecuteEfScope(db =>
        {
            // bulk update umbracoDocumentCultureVariation, once for edited = true, another for edited = false
            foreach (IGrouping<bool, DocumentCultureVariationRow> editValue in toUpdate.GroupBy(x => x.Edited))
            {
                foreach (IEnumerable<DocumentCultureVariationRow> batch in editValue.InGroupsOf(Constants.Sql.MaxParameterCount))
                {
                    db.Database.ExecuteSqlRaw(
                        $"UPDATE {Constants.DatabaseSchema.Tables.DocumentCultureVariation} SET edited = {(editValue.Key ? 1 : 0)} WHERE id IN ({string.Join(",", batch.Select(x => x.Id))})");
                }
            }

            // bulk update the umbracoDocument table
            foreach (IGrouping<bool, KeyValuePair<int, bool>> groupByValue in editedDocument.GroupBy(x => x.Value))
            {
                foreach (IEnumerable<KeyValuePair<int, bool>> batch in groupByValue.InGroupsOf(Constants.Sql.MaxParameterCount))
                {
                    db.Database.ExecuteSqlRaw(
                        $"UPDATE {Constants.DatabaseSchema.Tables.Document} SET edited = {(groupByValue.Key ? 1 : 0)} WHERE nodeId IN ({string.Join(",", batch.Select(x => x.Key))})");
                }
            }
        });
    }

    private void DeletePropertyType(IContentTypeComposition contentType, int propertyTypeId)
        => ExecuteEfScope(db =>
        {
            // first clear dependencies
            db.Database.ExecuteSqlRaw(
                $"DELETE FROM {Constants.DatabaseSchema.Tables.TagRelationship} WHERE propertyTypeId = {propertyTypeId}");
            db.Database.ExecuteSqlRaw(
                $"DELETE FROM {Constants.DatabaseSchema.Tables.PropertyData} WHERE propertyTypeId = {propertyTypeId}");

            // delete granular permissions scoped to this property type (permission format: "<propertyTypeKey>|...")
            Guid? propertyTypeKey = db.PropertyTypes
                .Where(x => x.Id == propertyTypeId)
                .Select(x => (Guid?)x.UniqueId)
                .FirstOrDefault();
            if (propertyTypeKey.HasValue)
            {
                db.Database.ExecuteSqlRaw(
                    $"DELETE FROM {Constants.DatabaseSchema.Tables.UserGroup2GranularPermission} WHERE uniqueId = {{0}} AND permission LIKE {{1}}",
                    contentType.Key,
                    $"{propertyTypeKey.Value}|%");
            }

            // Finally delete the property type.
            db.PropertyTypes.Where(x => x.ContentTypeId == contentType.Id && x.Id == propertyTypeId).ExecuteDelete();
        });

    /// <inheritdoc />
    public string GetUniqueAlias(string alias)
    {
        // alias is unique across ALL content types!
        List<string> aliases = ExecuteEfScope(db => db.ContentTypes
            .Where(x => x.Alias != null && x.Alias.StartsWith(alias))
            .Select(x => x.Alias!)
            .ToList());

        var i = 1;
        string test;
        while (aliases.Contains(test = alias + i))
        {
            i++;
        }

        return test;
    }

    /// <inheritdoc />
    public bool HasContainerInPath(string contentPath)
    {
        var ids = contentPath.Split(Constants.CharArrays.Comma)
            .Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        return HasContainerInPath(ids);
    }

    /// <inheritdoc />
    public bool HasContainerInPath(params int[] ids)
    {
        if (ids.Length == 0)
        {
            return false;
        }

        var sql =
            $"""
             SELECT COUNT(*) AS Value
             FROM {ContentTypeDto.TableName} ct
             INNER JOIN {Constants.DatabaseSchema.Tables.Content} c ON ct.{ContentTypeDto.NodeIdColumnName} = c.contentTypeId
             WHERE c.nodeId IN ({string.Join(",", ids)}) AND ct.listView IS NULL
             """;
        return ExecuteEfScope(db => db.Database.SqlQueryRaw<int>(sql).Single()) > 0;
    }

    /// <inheritdoc />
    public bool HasContentNodes(int id)
    {
        var sql =
            $"SELECT CASE WHEN EXISTS (SELECT * FROM {Constants.DatabaseSchema.Tables.Content} WHERE contentTypeId = {{0}}) THEN 1 ELSE 0 END AS Value";
        return ExecuteEfScope(db => db.Database.SqlQueryRaw<int>(sql, id).Single()) == 1;
    }

    /// <inheritdoc />
    public IEnumerable<Guid> GetAllowedParentKeys(Guid key)
        => ExecuteEfScope(db =>
        {
            IQueryable<int> childNodeIds = db.Nodes
                .Where(x => x.UniqueId == key)
                .Select(x => x.NodeId);

            return (from allowed in db.ContentTypeAllowedContentTypes
                    join node in db.Nodes on allowed.Id equals node.NodeId
                    where childNodeIds.Contains(allowed.AllowedId)
                    select node.UniqueId).ToList();
        });

    private (IContentType Entity, int SortOrder)[] GetAllowedContentTypes(IContentTypeBase contentTypeBase)
    {
        if (contentTypeBase.AllowedContentTypes?.Any() is not true)
        {
            return Array.Empty<(IContentType, int)>();
        }

        Guid[] allowedContentTypeKeys = contentTypeBase
            .AllowedContentTypes
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Key)
            .ToArray();

        // NOTE: we're efficiently discarding the input sort order here in favor of a "0 to n" sorting.
        return GetAllCached()
            .Where(c => allowedContentTypeKeys.Contains(c.Key))
            .Select(c => (c, allowedContentTypeKeys.IndexOf(c.Key)))
            .ToArray();
    }

    private sealed class PropertyValueVersionDto
    {
        private decimal? _decimalValue;

        public int VersionId { get; set; }

        public int PropertyTypeId { get; set; }

        public int? LanguageId { get; set; }

        public string? Segment { get; set; }

        public int? IntValue { get; set; }

        public decimal? DecimalValue
        {
            get => _decimalValue;
            set => _decimalValue = value?.Normalize();
        }

        public DateTime? DateValue { get; set; }

        public string? VarcharValue { get; set; }

        public string? TextValue { get; set; }

        public int NodeId { get; set; }

        public bool Current { get; set; }

        public bool Published { get; set; }

        public byte Variations { get; set; }
    }

    private sealed class DocumentCultureVariationRow
    {
        public int Id { get; set; }

        public int NodeId { get; set; }

        public int LanguageId { get; set; }

        public bool Edited { get; set; }
    }
}

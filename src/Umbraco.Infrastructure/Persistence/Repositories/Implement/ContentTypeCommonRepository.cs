using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Implements <see cref="IContentTypeCommonRepository" />.
/// </summary>
internal sealed class ContentTypeCommonRepository : IContentTypeCommonRepository
{
    private const string CacheKey =
        "Umbraco.Core.Persistence.Repositories.Implement.ContentTypeCommonRepository::AllTypes";

    private readonly AppCaches _appCaches;
    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _scopeAccessor;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemplateRepository _templateRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IContentTypeCommonRepository" /> class.
    /// </summary>
    public ContentTypeCommonRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        ITemplateRepository templateRepository,
        AppCaches appCaches,
        IShortStringHelper shortStringHelper)
    {
        _scopeAccessor = scopeAccessor;
        _templateRepository = templateRepository;
        _appCaches = appCaches;
        _shortStringHelper = shortStringHelper;
    }

    private IEFCoreScope<UmbracoDbContext> AmbientScope
        => _scopeAccessor.AmbientScope
           ?? throw new InvalidOperationException("Cannot run a repository without an ambient scope.");

    /// <inheritdoc />
    public IEnumerable<IContentTypeComposition>? GetAllTypes() =>
        // use a sliding cache - same as FullDataSet cache policy
        _appCaches.RuntimeCache.GetCacheItem(CacheKey, GetAllTypesInternal, RepositoryCacheConstants.DefaultCacheDuration, true);

    /// <inheritdoc />
    public void ClearCache() => _appCaches.RuntimeCache.Clear(CacheKey);

    private IEnumerable<IContentTypeComposition> GetAllTypesInternal()
    {
        // Touch the scope on the synchronous caller context before entering the async flow, ensuring that
        // async-local scope state (including bridge-scope enlistment) is captured on this execution context.
        EnsureAmbientScopeOnCallerContext();
        return GetAllTypesAsync().GetAwaiter().GetResult();
    }

    private void EnsureAmbientScopeOnCallerContext() => _ = AmbientScope;

    private async Task<IEnumerable<IContentTypeComposition>> GetAllTypesAsync()
    {
        return await AmbientScope.ExecuteWithContextAsync<IEnumerable<IContentTypeComposition>>(async db =>
        {
            // Query 1: content types with their nodes, ordered so pointer-walking below works
            List<ContentTypeDto> contentTypeDtos = await db.ContentTypes
                .Include(ct => ct.NodeDto)
                .OrderBy(ct => ct.NodeId)
                .ToListAsync();

            if (contentTypeDtos.Count == 0)
            {
                return [];
            }

            // Query 2: allowed content types (parent content type id is ordered to align with Query 1)
            List<ContentTypeAllowedContentTypeDto> allowedDtos = await db.ContentTypeAllowedContentTypes
                .OrderBy(a => a.Id)
                .ToListAsync();

            // Query 3: property type groups, ordered for the group pointer-walk in MapGroupsAndProperties
            List<PropertyTypeGroupDto> groupDtos = await db.PropertyTypeGroups
                .OrderBy(g => g.ContentTypeNodeId)
                .ThenBy(g => g.SortOrder)
                .ThenBy(g => g.Id)
                .ToListAsync();

            // Query 4: all property types with data type info and optional member-specific metadata
            List<PropertyTypeDto> propertyDtos = await db.PropertyTypes
                .Include(pt => pt.DataTypeDto).ThenInclude(dt => dt.NodeDto)
                .Include(pt => pt.MemberPropertyTypeDto)
                .OrderBy(pt => pt.ContentTypeId)
                .ThenBy(pt => pt.SortOrder)
                .ThenBy(pt => pt.Id)
                .ToListAsync();

            // Query 5: composition (parent-child content type hierarchy)
            List<ContentType2ContentTypeDto> compositionDtos = await db.ContentTypeComposition
                .OrderBy(c => c.ChildId)
                .ToListAsync();

            // Query 6: template associations (document types only)
            List<ContentTypeTemplateDto> templateDtos = await db.ContentTypeTemplates
                .OrderBy(t => t.ContentTypeNodeId)
                .ToListAsync();

            // Query 7: per-content-type version cleanup policies (document types only)
            List<ContentVersionCleanupPolicyDto> cleanupDtos = await db.ContentVersionCleanupPolicies
                .OrderBy(p => p.ContentTypeId)
                .ToListAsync();

            // Build alias and key lookups for resolving AllowedContentTypes
            var aliases = contentTypeDtos.ToDictionary(x => x.NodeId, x => x.Alias);
            var keys = contentTypeDtos.ToDictionary(x => x.NodeId, x => x.NodeDto.UniqueId);

            // Instantiate content type entities and wire up allowed child types
            var contentTypes = new Dictionary<int, IContentTypeComposition>(contentTypeDtos.Count);
            var allowedIx = 0;
            foreach (ContentTypeDto dto in contentTypeDtos)
            {
                IContentTypeComposition contentType;
                if (dto.NodeDto.NodeObjectType == Constants.ObjectTypes.MediaType)
                {
                    contentType = ContentTypeFactory.BuildMediaTypeEntity(_shortStringHelper, dto);
                }
                else if (dto.NodeDto.NodeObjectType == Constants.ObjectTypes.DocumentType)
                {
                    contentType = ContentTypeFactory.BuildContentTypeEntity(_shortStringHelper, dto);
                }
                else if (dto.NodeDto.NodeObjectType == Constants.ObjectTypes.MemberType)
                {
                    contentType = ContentTypeFactory.BuildMemberTypeEntity(_shortStringHelper, dto);
                }
                else
                {
                    throw new PanicException($"The node object type {dto.NodeDto.NodeObjectType} is not supported");
                }

                contentTypes.Add(contentType.Id, contentType);

                var allowedContentTypes = new List<ContentTypeSort>();
                while (allowedIx < allowedDtos.Count && allowedDtos[allowedIx].Id == dto.NodeId)
                {
                    ContentTypeAllowedContentTypeDto allowedDto = allowedDtos[allowedIx];
                    if (aliases.TryGetValue(allowedDto.AllowedId, out var alias)
                        && keys.TryGetValue(allowedDto.AllowedId, out Guid key))
                    {
                        allowedContentTypes.Add(new ContentTypeSort(key, allowedDto.SortOrder, alias!));
                    }

                    allowedIx++;
                }

                contentType.AllowedContentTypes = allowedContentTypes;
            }

            MapTemplates(contentTypes, templateDtos);
            MapComposition(contentTypes, compositionDtos);
            MapGroupsAndProperties(contentTypes, groupDtos, propertyDtos);
            MapHistoryCleanup(contentTypes, cleanupDtos);

            foreach (IContentTypeComposition contentType in contentTypes.Values)
            {
                contentType.ResetDirtyProperties(false);
            }

            return contentTypes.Values;
        });
    }

    private static void MapHistoryCleanup(
        Dictionary<int, IContentTypeComposition> contentTypes,
        List<ContentVersionCleanupPolicyDto> cleanupDtos)
    {
        Dictionary<int, ContentVersionCleanupPolicyDto> cleanupByContentTypeId =
            cleanupDtos.ToDictionary(x => x.ContentTypeId);

        foreach (IContentTypeComposition c in contentTypes.Values)
        {
            if (c is not ContentType contentType)
            {
                continue;
            }

            var historyCleanup = new HistoryCleanup();

            if (cleanupByContentTypeId.TryGetValue(contentType.Id, out ContentVersionCleanupPolicyDto? versionCleanup))
            {
                historyCleanup.PreventCleanup = versionCleanup.PreventCleanup;
                historyCleanup.KeepAllVersionsNewerThanDays = versionCleanup.KeepAllVersionsNewerThanDays;
                historyCleanup.KeepLatestVersionPerDayForDays = versionCleanup.KeepLatestVersionPerDayForDays;
            }

            contentType.HistoryCleanup = historyCleanup;
        }
    }

    private void MapTemplates(
        Dictionary<int, IContentTypeComposition> contentTypes,
        List<ContentTypeTemplateDto> templateDtos)
    {
        IEnumerable<ITemplate>? allTemplates = _templateRepository.GetMany((int[]?)null);
        Dictionary<int, ITemplate> templates = allTemplates.ToDictionary(x => x.Id, x => x);

        Dictionary<int, List<ContentTypeTemplateDto>> templatesByContentTypeId = templateDtos
            .GroupBy(t => t.ContentTypeNodeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (IContentTypeComposition c in contentTypes.Values)
        {
            if (c is not IContentType contentType)
            {
                continue;
            }

            var allowedTemplates = new List<ITemplate>();
            var defaultTemplateId = 0;

            if (templatesByContentTypeId.TryGetValue(contentType.Id, out List<ContentTypeTemplateDto>? contentTypeTemplateDtos))
            {
                foreach (ContentTypeTemplateDto templateDto in contentTypeTemplateDtos)
                {
                    if (!templates.TryGetValue(templateDto.TemplateNodeId, out ITemplate? template))
                    {
                        continue;
                    }

                    allowedTemplates.Add(template);

                    if (templateDto.IsDefault)
                    {
                        defaultTemplateId = template.Id;
                    }
                }
            }

            contentType.AllowedTemplates = allowedTemplates;
            contentType.DefaultTemplateId = defaultTemplateId;
        }
    }

    private static void MapComposition(
        IDictionary<int, IContentTypeComposition> contentTypes,
        List<ContentType2ContentTypeDto> compositionDtos)
    {
        Dictionary<int, List<ContentType2ContentTypeDto>> compositionByChildId = compositionDtos
            .GroupBy(c => c.ChildId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (IContentTypeComposition contentType in contentTypes.Values)
        {
            if (!compositionByChildId.TryGetValue(contentType.Id, out List<ContentType2ContentTypeDto>? parents))
            {
                continue;
            }

            foreach (ContentType2ContentTypeDto parentDto in parents)
            {
                if (contentTypes.TryGetValue(parentDto.ParentId, out IContentTypeComposition? parentContentType))
                {
                    contentType.AddContentType(parentContentType);
                }
            }
        }
    }

    private void MapGroupsAndProperties(
        IDictionary<int, IContentTypeComposition> contentTypes,
        List<PropertyTypeGroupDto> groupDtos,
        List<PropertyTypeDto> propertyDtos)
    {
        Dictionary<string, PropertyType> builtinProperties =
            ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper);

        // Index grouped properties by group id, sorted by position within each group
        Dictionary<int, List<PropertyTypeDto>> propertiesByGroupId = propertyDtos
            .Where(pt => pt.PropertyTypeGroupId.HasValue)
            .GroupBy(pt => pt.PropertyTypeGroupId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(pt => pt.SortOrder).ThenBy(pt => pt.Id).ToList());

        // Index group-less properties by content type id, sorted by position
        Dictionary<int, List<PropertyTypeDto>> noGroupPropertiesByContentTypeId = propertyDtos
            .Where(pt => !pt.PropertyTypeGroupId.HasValue)
            .GroupBy(pt => pt.ContentTypeId)
            .ToDictionary(g => g.Key, g => g.OrderBy(pt => pt.SortOrder).ThenBy(pt => pt.Id).ToList());

        // Index groups by content type id (already ordered: ContentTypeNodeId → SortOrder → Id)
        Dictionary<int, List<PropertyTypeGroupDto>> groupsByContentTypeId = groupDtos
            .GroupBy(g => g.ContentTypeNodeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (IContentTypeComposition contentType in contentTypes.Values)
        {
            var isPublishing = contentType is IContentType;

            // Group-less properties
            var noGroupPropertyTypes = new PropertyTypeCollection(isPublishing);
            if (noGroupPropertiesByContentTypeId.TryGetValue(contentType.Id, out List<PropertyTypeDto>? ungroupedProps))
            {
                foreach (PropertyTypeDto pt in ungroupedProps)
                {
                    noGroupPropertyTypes.Add(MapPropertyType(contentType, pt, builtinProperties));
                }
            }

            // Property groups and their properties
            var groupCollection = new PropertyGroupCollection();
            if (groupsByContentTypeId.TryGetValue(contentType.Id, out List<PropertyTypeGroupDto>? groups))
            {
                foreach (PropertyTypeGroupDto groupDto in groups)
                {
                    PropertyGroup group = MapPropertyGroup(groupDto, isPublishing);
                    groupCollection.Add(group);

                    if (propertiesByGroupId.TryGetValue(groupDto.Id, out List<PropertyTypeDto>? groupProps))
                    {
                        foreach (PropertyTypeDto pt in groupProps)
                        {
                            group.PropertyTypes?.Add(MapPropertyType(contentType, pt, builtinProperties));
                        }
                    }
                }
            }

            contentType.PropertyGroups = groupCollection;
            contentType.NoGroupPropertyTypes = noGroupPropertyTypes;

            // Ensure standard built-in property types exist on member types
            if (contentType is IMemberType memberType)
            {
                foreach ((var alias, PropertyType propertyType) in builtinProperties)
                {
                    var added = memberType.AddPropertyType(
                        propertyType,
                        Constants.Conventions.Member.StandardPropertiesGroupAlias,
                        Constants.Conventions.Member.StandardPropertiesGroupName);

                    if (added)
                    {
                        memberType.SetIsSensitiveProperty(alias, false);
                        memberType.SetMemberCanEditProperty(alias, false);
                        memberType.SetMemberCanViewProperty(alias, false);
                    }
                }
            }
        }
    }

    private static PropertyGroup MapPropertyGroup(PropertyTypeGroupDto dto, bool isPublishing) =>
        new(new PropertyTypeCollection(isPublishing))
        {
            Id = dto.Id,
            Key = dto.UniqueId,
            Type = (PropertyGroupType)dto.Type,
            Name = dto.Text,
            Alias = dto.Alias,
            SortOrder = dto.SortOrder,
        };

    private PropertyType MapPropertyType(
        IContentTypeComposition contentType,
        PropertyTypeDto dto,
        IDictionary<string, PropertyType> builtinProperties)
    {
        var groupId = dto.PropertyTypeGroupId;

        var readonlyStorageType = builtinProperties.TryGetValue(dto.Alias!, out PropertyType? propertyType);
        ValueStorageType storageType = readonlyStorageType
            ? propertyType!.ValueStorageType
            : Enum<ValueStorageType>.Parse(dto.DataTypeDto.DbType);

        if (contentType is IMemberType memberType && dto.Alias is not null)
        {
            memberType.SetIsSensitiveProperty(dto.Alias, dto.MemberPropertyTypeDto?.IsSensitive ?? false);
            memberType.SetMemberCanEditProperty(dto.Alias, dto.MemberPropertyTypeDto?.CanEdit ?? false);
            memberType.SetMemberCanViewProperty(dto.Alias, dto.MemberPropertyTypeDto?.ViewOnProfile ?? false);
        }

        return new PropertyType(_shortStringHelper, dto.DataTypeDto.EditorAlias, storageType, readonlyStorageType, dto.Alias)
        {
            Description = dto.Description,
            DataTypeId = dto.DataTypeId,
            DataTypeKey = dto.DataTypeDto.NodeDto.UniqueId,
            Id = dto.Id,
            Key = dto.UniqueId,
            Mandatory = dto.Mandatory,
            MandatoryMessage = dto.MandatoryMessage,
            Name = dto.Name ?? string.Empty,
            PropertyGroupId = groupId.HasValue ? new Lazy<int>(() => groupId.Value) : null,
            SortOrder = dto.SortOrder,
            ValidationRegExp = dto.ValidationRegExp,
            ValidationRegExpMessage = dto.ValidationRegExpMessage,
            Variations = (ContentVariation)dto.Variations,
            LabelOnTop = dto.LabelOnTop,
        };
    }
}

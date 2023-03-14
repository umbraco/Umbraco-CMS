using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Implements <see cref="IContentTypeCommonRepository" />.
/// </summary>
internal class ContentTypeCommonRepository : IContentTypeCommonRepository
{
    private const string CacheKey =
        "Umbraco.Core.Persistence.Repositories.Implement.ContentTypeCommonRepository::AllTypes";

    private readonly AppCaches _appCaches;
    private readonly IScopeAccessor _scopeAccessor;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemplateRepository _templateRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IContentTypeCommonRepository" /> class.
    /// </summary>
    public ContentTypeCommonRepository(IScopeAccessor scopeAccessor, ITemplateRepository templateRepository,
        AppCaches appCaches, IShortStringHelper shortStringHelper)
    {
        _scopeAccessor = scopeAccessor;
        _templateRepository = templateRepository;
        _appCaches = appCaches;
        _shortStringHelper = shortStringHelper;
    }

    private IScope? AmbientScope => _scopeAccessor.AmbientScope;

    private IUmbracoDatabase? Database => AmbientScope?.Database;

    private ISqlContext? SqlContext => AmbientScope?.SqlContext;

    // private Sql<ISqlContext> Sql(string sql, params object[] args) => SqlContext.Sql(sql, args);
    // private ISqlSyntaxProvider SqlSyntax => SqlContext.SqlSyntax;
    // private IQuery<T> Query<T>() => SqlContext.Query<T>();

    /// <inheritdoc />
    public IEnumerable<IContentTypeComposition>? GetAllTypes() =>

        // use a 5 minutes sliding cache - same as FullDataSet cache policy
        _appCaches.RuntimeCache.GetCacheItem(CacheKey, GetAllTypesInternal, TimeSpan.FromMinutes(5), true);

    /// <inheritdoc />
    public void ClearCache() => _appCaches.RuntimeCache.Clear(CacheKey);

    private Sql<ISqlContext>? Sql() => SqlContext?.Sql();

    private IEnumerable<IContentTypeComposition> GetAllTypesInternal()
    {
        var contentTypes = new Dictionary<int, IContentTypeComposition>();

        // get content types
        Sql<ISqlContext>? sql1 = Sql()?
            .Select<ContentTypeDto>(r => r.Select(x => x.NodeDto))
            .From<ContentTypeDto>()
            .InnerJoin<NodeDto>().On<ContentTypeDto, NodeDto>((ct, n) => ct.NodeId == n.NodeId)
            .OrderBy<ContentTypeDto>(x => x.NodeId);

        List<ContentTypeDto>? contentTypeDtos = Database?.Fetch<ContentTypeDto>(sql1);

        // get allowed content types
        Sql<ISqlContext>? sql2 = Sql()?
            .Select<ContentTypeAllowedContentTypeDto>()
            .From<ContentTypeAllowedContentTypeDto>()
            .OrderBy<ContentTypeAllowedContentTypeDto>(x => x.Id);

        List<ContentTypeAllowedContentTypeDto>? allowedDtos = Database?.Fetch<ContentTypeAllowedContentTypeDto>(sql2);

        if (contentTypeDtos is null)
        {
            return contentTypes.Values;
        }

        // prepare
        // note: same alias could be used for media, content... but always different ids = ok
        var aliases = contentTypeDtos.ToDictionary(x => x.NodeId, x => x.Alias);

        // create
        var allowedDtoIx = 0;
        foreach (ContentTypeDto contentTypeDto in contentTypeDtos)
        {
            // create content type
            IContentTypeComposition contentType;
            if (contentTypeDto.NodeDto.NodeObjectType == Constants.ObjectTypes.MediaType)
            {
                contentType = ContentTypeFactory.BuildMediaTypeEntity(_shortStringHelper, contentTypeDto);
            }
            else if (contentTypeDto.NodeDto.NodeObjectType == Constants.ObjectTypes.DocumentType)
            {
                contentType = ContentTypeFactory.BuildContentTypeEntity(_shortStringHelper, contentTypeDto);
            }
            else if (contentTypeDto.NodeDto.NodeObjectType == Constants.ObjectTypes.MemberType)
            {
                contentType = ContentTypeFactory.BuildMemberTypeEntity(_shortStringHelper, contentTypeDto);
            }
            else
            {
                throw new PanicException(
                    $"The node object type {contentTypeDto.NodeDto.NodeObjectType} is not supported");
            }

            contentTypes.Add(contentType.Id, contentType);

            // map allowed content types
            var allowedContentTypes = new List<ContentTypeSort>();
            while (allowedDtoIx < allowedDtos?.Count && allowedDtos[allowedDtoIx].Id == contentTypeDto.NodeId)
            {
                ContentTypeAllowedContentTypeDto allowedDto = allowedDtos[allowedDtoIx];
                if (!aliases.TryGetValue(allowedDto.AllowedId, out var alias))
                {
                    continue;
                }

                allowedContentTypes.Add(new ContentTypeSort(
                    new Lazy<int>(() => allowedDto.AllowedId),
                    allowedDto.SortOrder, alias!));
                allowedDtoIx++;
            }

            contentType.AllowedContentTypes = allowedContentTypes;
        }

        MapTemplates(contentTypes);
        MapComposition(contentTypes);
        MapGroupsAndProperties(contentTypes);
        MapHistoryCleanup(contentTypes);

        // finalize
        foreach (IContentTypeComposition contentType in contentTypes.Values)
        {
            contentType.ResetDirtyProperties(false);
        }

        return contentTypes.Values;
    }

    private void MapHistoryCleanup(Dictionary<int, IContentTypeComposition> contentTypes)
    {
        // get templates
        Sql<ISqlContext>? sql1 = Sql()?
            .Select<ContentVersionCleanupPolicyDto>()
            .From<ContentVersionCleanupPolicyDto>()
            .OrderBy<ContentVersionCleanupPolicyDto>(x => x.ContentTypeId);

        List<ContentVersionCleanupPolicyDto>? contentVersionCleanupPolicyDtos =
            Database?.Fetch<ContentVersionCleanupPolicyDto>(sql1);

        var contentVersionCleanupPolicyDictionary =
            contentVersionCleanupPolicyDtos?.ToDictionary(x => x.ContentTypeId);
        foreach (IContentTypeComposition c in contentTypes.Values)
        {
            if (!(c is ContentType contentType))
            {
                continue;
            }

            var historyCleanup = new HistoryCleanup();

            if (contentVersionCleanupPolicyDictionary is not null &&
                contentVersionCleanupPolicyDictionary.TryGetValue(
                    contentType.Id,
                    out ContentVersionCleanupPolicyDto? versionCleanup))
            {
                historyCleanup.PreventCleanup = versionCleanup.PreventCleanup;
                historyCleanup.KeepAllVersionsNewerThanDays = versionCleanup.KeepAllVersionsNewerThanDays;
                historyCleanup.KeepLatestVersionPerDayForDays = versionCleanup.KeepLatestVersionPerDayForDays;
            }

            contentType.HistoryCleanup = historyCleanup;
        }
    }

    private void MapTemplates(Dictionary<int, IContentTypeComposition> contentTypes)
    {
        // get templates
        Sql<ISqlContext>? sql1 = Sql()?
            .Select<ContentTypeTemplateDto>()
            .From<ContentTypeTemplateDto>()
            .OrderBy<ContentTypeTemplateDto>(x => x.ContentTypeNodeId);

        List<ContentTypeTemplateDto>? templateDtos = Database?.Fetch<ContentTypeTemplateDto>(sql1);

        // var templates = templateRepository.GetMany(templateDtos.Select(x => x.TemplateNodeId).ToArray()).ToDictionary(x => x.Id, x => x);
        IEnumerable<ITemplate>? allTemplates = _templateRepository.GetMany();

        var templates = allTemplates.ToDictionary(x => x.Id, x => x);
        var templateDtoIx = 0;

        foreach (IContentTypeComposition c in contentTypes.Values)
        {
            if (!(c is IContentType contentType))
            {
                continue;
            }

            // map allowed templates
            var allowedTemplates = new List<ITemplate>();
            var defaultTemplateId = 0;
            while (templateDtoIx < templateDtos?.Count &&
                   templateDtos[templateDtoIx].ContentTypeNodeId == contentType.Id)
            {
                ContentTypeTemplateDto allowedDto = templateDtos[templateDtoIx];
                templateDtoIx++;
                if (!templates.TryGetValue(allowedDto.TemplateNodeId, out ITemplate? template))
                {
                    continue;
                }

                allowedTemplates.Add(template);

                if (allowedDto.IsDefault)
                {
                    defaultTemplateId = template.Id;
                }
            }

            contentType.AllowedTemplates = allowedTemplates;
            contentType.DefaultTemplateId = defaultTemplateId;
        }
    }

    private void MapComposition(IDictionary<int, IContentTypeComposition> contentTypes)
    {
        // get parent/child
        Sql<ISqlContext>? sql1 = Sql()?
            .Select<ContentType2ContentTypeDto>()
            .From<ContentType2ContentTypeDto>()
            .OrderBy<ContentType2ContentTypeDto>(x => x.ChildId);

        List<ContentType2ContentTypeDto>? compositionDtos = Database?.Fetch<ContentType2ContentTypeDto>(sql1);

        // map
        var compositionIx = 0;
        foreach (IContentTypeComposition contentType in contentTypes.Values)
        {
            while (compositionIx < compositionDtos?.Count &&
                   compositionDtos[compositionIx].ChildId == contentType.Id)
            {
                ContentType2ContentTypeDto parentDto = compositionDtos[compositionIx];
                compositionIx++;

                if (!contentTypes.TryGetValue(parentDto.ParentId, out IContentTypeComposition? parentContentType))
                {
                    continue;
                }

                contentType.AddContentType(parentContentType);
            }
        }
    }

    private void MapGroupsAndProperties(IDictionary<int, IContentTypeComposition> contentTypes)
    {
        Sql<ISqlContext>? sql1 = Sql()?
            .Select<PropertyTypeGroupDto>()
            .From<PropertyTypeGroupDto>()
            .InnerJoin<ContentTypeDto>()
            .On<PropertyTypeGroupDto, ContentTypeDto>((ptg, ct) => ptg.ContentTypeNodeId == ct.NodeId)
            .OrderBy<ContentTypeDto>(x => x.NodeId)
            .AndBy<PropertyTypeGroupDto>(x => x.SortOrder, x => x.Id);

        List<PropertyTypeGroupDto>? groupDtos = Database?.Fetch<PropertyTypeGroupDto>(sql1);

        Sql<ISqlContext>? sql2 = Sql()?
            .Select<PropertyTypeDto>(r => r.Select(x => x.DataTypeDto, r1 => r1.Select(x => x.NodeDto)))
            .AndSelect<MemberPropertyTypeDto>()
            .From<PropertyTypeDto>()
            .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((pt, dt) => pt.DataTypeId == dt.NodeId)
            .InnerJoin<NodeDto>().On<DataTypeDto, NodeDto>((dt, n) => dt.NodeId == n.NodeId)
            .InnerJoin<ContentTypeDto>()
            .On<PropertyTypeDto, ContentTypeDto>((pt, ct) => pt.ContentTypeId == ct.NodeId)
            .LeftJoin<PropertyTypeGroupDto>()
            .On<PropertyTypeDto, PropertyTypeGroupDto>((pt, ptg) => pt.PropertyTypeGroupId == ptg.Id)
            .LeftJoin<MemberPropertyTypeDto>()
            .On<PropertyTypeDto, MemberPropertyTypeDto>((pt, mpt) => pt.Id == mpt.PropertyTypeId)
            .OrderBy<ContentTypeDto>(x => x.NodeId)
            .AndBy<
                PropertyTypeGroupDto>(
                    x => x.SortOrder,
                    x => x.Id) // NULLs will come first or last, never mind, we deal with it below
            .AndBy<PropertyTypeDto>(x => x.SortOrder, x => x.Id);

        List<PropertyTypeCommonDto>? propertyDtos = Database?.Fetch<PropertyTypeCommonDto>(sql2);
        Dictionary<string, PropertyType> builtinProperties =
            ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper);

        var groupIx = 0;
        var propertyIx = 0;
        foreach (IContentTypeComposition contentType in contentTypes.Values)
        {
            // only IContentType is publishing
            var isPublishing = contentType is IContentType;

            // get group-less properties (in case NULL is ordered first)
            var noGroupPropertyTypes = new PropertyTypeCollection(isPublishing);
            while (propertyIx < propertyDtos?.Count && propertyDtos[propertyIx].ContentTypeId == contentType.Id &&
                   propertyDtos[propertyIx].PropertyTypeGroupId == null)
            {
                noGroupPropertyTypes.Add(MapPropertyType(contentType, propertyDtos[propertyIx], builtinProperties));
                propertyIx++;
            }

            // get groups and their properties
            var groupCollection = new PropertyGroupCollection();
            while (groupIx < groupDtos?.Count && groupDtos[groupIx].ContentTypeNodeId == contentType.Id)
            {
                PropertyGroup group = MapPropertyGroup(groupDtos[groupIx], isPublishing);
                groupCollection.Add(group);
                groupIx++;

                while (propertyIx < propertyDtos?.Count &&
                       propertyDtos[propertyIx].ContentTypeId == contentType.Id &&
                       propertyDtos[propertyIx].PropertyTypeGroupId == group.Id)
                {
                    group.PropertyTypes?.Add(MapPropertyType(contentType, propertyDtos[propertyIx],
                        builtinProperties));
                    propertyIx++;
                }
            }

            contentType.PropertyGroups = groupCollection;

            // get group-less properties (in case NULL is ordered last)
            while (propertyIx < propertyDtos?.Count && propertyDtos[propertyIx].ContentTypeId == contentType.Id &&
                   propertyDtos[propertyIx].PropertyTypeGroupId == null)
            {
                noGroupPropertyTypes.Add(MapPropertyType(contentType, propertyDtos[propertyIx], builtinProperties));
                propertyIx++;
            }

            contentType.NoGroupPropertyTypes = noGroupPropertyTypes;

            // ensure builtin properties
            if (contentType is IMemberType memberType)
            {
                // ensure that property types exist (ok if they already exist)
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

    private PropertyGroup MapPropertyGroup(PropertyTypeGroupDto dto, bool isPublishing) =>
        new(new PropertyTypeCollection(isPublishing))
        {
            Id = dto.Id,
            Key = dto.UniqueId,
            Type = (PropertyGroupType)dto.Type,
            Name = dto.Text,
            Alias = dto.Alias,
            SortOrder = dto.SortOrder,
        };

    private PropertyType MapPropertyType(IContentTypeComposition contentType, PropertyTypeCommonDto dto,
        IDictionary<string, PropertyType> builtinProperties)
    {
        var groupId = dto.PropertyTypeGroupId;

        var readonlyStorageType = builtinProperties.TryGetValue(dto.Alias!, out PropertyType? propertyType);
        ValueStorageType storageType = readonlyStorageType
            ? propertyType!.ValueStorageType
            : Enum<ValueStorageType>.Parse(dto.DataTypeDto.DbType);

        if (contentType is IMemberType memberType && dto.Alias is not null)
        {
            memberType.SetIsSensitiveProperty(dto.Alias, dto.IsSensitive);
            memberType.SetMemberCanEditProperty(dto.Alias, dto.CanEdit);
            memberType.SetMemberCanViewProperty(dto.Alias, dto.ViewOnProfile);
        }

        return new
            PropertyType(_shortStringHelper, dto.DataTypeDto.EditorAlias, storageType, readonlyStorageType,
                dto.Alias)
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

using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Scoping;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Implements <see cref="IContentTypeCommonRepository"/>.
    /// </summary>
    internal class ContentTypeCommonRepository : IContentTypeCommonRepository
    {
        private const string CacheKey = "Umbraco.Core.Persistence.Repositories.Implement.ContentTypeCommonRepository::AllTypes";

        private readonly AppCaches _appCaches;
        private readonly IScopeAccessor _scopeAccessor;
        private readonly ITemplateRepository _templateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="IContentTypeCommonRepository"/> class.
        /// </summary>
        public ContentTypeCommonRepository(IScopeAccessor scopeAccessor, ITemplateRepository templateRepository, AppCaches appCaches)
        {
            _scopeAccessor = scopeAccessor;
            _templateRepository = templateRepository;
            _appCaches = appCaches;
        }

        private IScope AmbientScope => _scopeAccessor.AmbientScope;
        private IUmbracoDatabase Database => AmbientScope.Database;
        private ISqlContext SqlContext => AmbientScope.SqlContext;
        private Sql<ISqlContext> Sql() => SqlContext.Sql();
        //private Sql<ISqlContext> Sql(string sql, params object[] args) => SqlContext.Sql(sql, args);
        //private ISqlSyntaxProvider SqlSyntax => SqlContext.SqlSyntax;
        //private IQuery<T> Query<T>() => SqlContext.Query<T>();

        /// <inheritdoc />
        public IEnumerable<IContentTypeComposition> GetAllTypes()
        {
            // use a 5 minutes sliding cache - same as FullDataSet cache policy
            return _appCaches.RuntimeCache.GetCacheItem(CacheKey, GetAllTypesInternal, TimeSpan.FromMinutes(5), true);
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            _appCaches.RuntimeCache.Clear(CacheKey);
        }

        private IEnumerable<IContentTypeComposition> GetAllTypesInternal()
        {
            var contentTypes = new Dictionary<int, IContentTypeComposition>();

            // get content types
            var sql1 = Sql()
                .Select<ContentTypeDto>(r => r.Select(x => x.NodeDto))
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>().On<ContentTypeDto, NodeDto>((ct, n) => ct.NodeId == n.NodeId)
                .OrderBy<ContentTypeDto>(x => x.NodeId);

            var contentTypeDtos = Database.Fetch<ContentTypeDto>(sql1);

            // get allowed content types
            var sql2 = Sql()
                .Select<ContentTypeAllowedContentTypeDto>()
                .From<ContentTypeAllowedContentTypeDto>()
                .OrderBy<ContentTypeAllowedContentTypeDto>(x => x.Id);

            var allowedDtos = Database.Fetch<ContentTypeAllowedContentTypeDto>(sql2);

            // prepare
            // note: same alias could be used for media, content... but always different ids = ok
            var aliases = contentTypeDtos.ToDictionary(x => x.NodeId, x => x.Alias);

            // create
            var allowedDtoIx = 0;
            foreach (var contentTypeDto in contentTypeDtos)
            {
                // create content type
                IContentTypeComposition contentType;
                if (contentTypeDto.NodeDto.NodeObjectType == Constants.ObjectTypes.MediaType)
                    contentType = ContentTypeFactory.BuildMediaTypeEntity(contentTypeDto);
                else if (contentTypeDto.NodeDto.NodeObjectType == Constants.ObjectTypes.DocumentType)
                    contentType = ContentTypeFactory.BuildContentTypeEntity(contentTypeDto);
                else if (contentTypeDto.NodeDto.NodeObjectType == Constants.ObjectTypes.MemberType)
                    contentType = ContentTypeFactory.BuildMemberTypeEntity(contentTypeDto);
                else throw new PanicException($"The node object type {contentTypeDto.NodeDto.NodeObjectType} is not supported");
                contentTypes.Add(contentType.Id, contentType);

                // map allowed content types
                var allowedContentTypes = new List<ContentTypeSort>();
                while (allowedDtoIx < allowedDtos.Count && allowedDtos[allowedDtoIx].Id == contentTypeDto.NodeId)
                {
                    var allowedDto = allowedDtos[allowedDtoIx];
                    if (!aliases.TryGetValue(allowedDto.AllowedId, out var alias)) continue;
                    allowedContentTypes.Add(new ContentTypeSort(new Lazy<int>(() => allowedDto.AllowedId), allowedDto.SortOrder, alias));
                    allowedDtoIx++;
                }
                contentType.AllowedContentTypes = allowedContentTypes;
            }

            MapTemplates(contentTypes);
            MapComposition(contentTypes);
            MapGroupsAndProperties(contentTypes);
            MapHistoryCleanup(contentTypes);

            // finalize
            foreach (var contentType in contentTypes.Values)
            {
                contentType.ResetDirtyProperties(false);
            }

            return contentTypes.Values;
        }

        private void MapHistoryCleanup(Dictionary<int, IContentTypeComposition> contentTypes)
        {
            // get templates
            var sql1 = Sql()
                .Select<ContentVersionCleanupPolicyDto>()
                .From<ContentVersionCleanupPolicyDto>()
                .OrderBy<ContentVersionCleanupPolicyDto>(x => x.ContentTypeId);

            var contentVersionCleanupPolicyDtos = Database.Fetch<ContentVersionCleanupPolicyDto>(sql1);

            var contentVersionCleanupPolicyDictionary =
                contentVersionCleanupPolicyDtos.ToDictionary(x => x.ContentTypeId);
            foreach (var c in contentTypes.Values)
            {
                if (!(c is ContentType contentType)) continue;

                var historyCleanup = new HistoryCleanup();

                if (contentVersionCleanupPolicyDictionary.TryGetValue(contentType.Id, out var versionCleanup))
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
            var sql1 = Sql()
                .Select<ContentTypeTemplateDto>()
                .From<ContentTypeTemplateDto>()
                .OrderBy<ContentTypeTemplateDto>(x => x.ContentTypeNodeId);

            var templateDtos = Database.Fetch<ContentTypeTemplateDto>(sql1);
            //var templates = templateRepository.GetMany(templateDtos.Select(x => x.TemplateNodeId).ToArray()).ToDictionary(x => x.Id, x => x);
            var templates = _templateRepository.GetMany().ToDictionary(x => x.Id, x => x);
            var templateDtoIx = 0;

            foreach (var c in contentTypes.Values)
            {
                if (!(c is ContentType contentType)) continue;

                // map allowed templates
                var allowedTemplates = new List<ITemplate>();
                var defaultTemplateId = 0;
                while (templateDtoIx < templateDtos.Count && templateDtos[templateDtoIx].ContentTypeNodeId == contentType.Id)
                {
                    var allowedDto = templateDtos[templateDtoIx];
                    templateDtoIx++;
                    if (!templates.TryGetValue(allowedDto.TemplateNodeId, out var template)) continue;
                    allowedTemplates.Add(template);

                    if (allowedDto.IsDefault)
                        defaultTemplateId = template.Id;
                }
                contentType.AllowedTemplates = allowedTemplates;
                contentType.DefaultTemplateId = defaultTemplateId;
            }
        }

        private void MapComposition(IDictionary<int, IContentTypeComposition> contentTypes)
        {
            // get parent/child
            var sql1 = Sql()
                .Select<ContentType2ContentTypeDto>()
                .From<ContentType2ContentTypeDto>()
                .OrderBy<ContentType2ContentTypeDto>(x => x.ChildId);

            var compositionDtos = Database.Fetch<ContentType2ContentTypeDto>(sql1);

            // map
            var compositionIx = 0;
            foreach (var contentType in contentTypes.Values)
            {
                while (compositionIx < compositionDtos.Count && compositionDtos[compositionIx].ChildId == contentType.Id)
                {
                    var parentDto = compositionDtos[compositionIx];
                    compositionIx++;

                    if (!contentTypes.TryGetValue(parentDto.ParentId, out var parentContentType))
                        continue;
                    contentType.AddContentType(parentContentType);
                }
            }
        }

        private void MapGroupsAndProperties(IDictionary<int, IContentTypeComposition> contentTypes)
        {
            var sql1 = Sql()
                .Select<PropertyTypeGroupDto>()
                .From<PropertyTypeGroupDto>()
                .InnerJoin<ContentTypeDto>().On<PropertyTypeGroupDto, ContentTypeDto>((ptg, ct) => ptg.ContentTypeNodeId == ct.NodeId)
                .OrderBy<ContentTypeDto>(x => x.NodeId)
                .AndBy<PropertyTypeGroupDto>(x => x.SortOrder, x => x.Id);

            var groupDtos = Database.Fetch<PropertyTypeGroupDto>(sql1);

            var sql2 = Sql()
                .Select<PropertyTypeDto>(r => r.Select(x => x.DataTypeDto, r1 => r1.Select(x => x.NodeDto)))
                .AndSelect<MemberPropertyTypeDto>()
                .From<PropertyTypeDto>()
                .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((pt, dt) => pt.DataTypeId == dt.NodeId)
                .InnerJoin<NodeDto>().On<DataTypeDto, NodeDto>((dt, n) => dt.NodeId == n.NodeId)
                .InnerJoin<ContentTypeDto>().On<PropertyTypeDto, ContentTypeDto>((pt, ct) => pt.ContentTypeId == ct.NodeId)
                .LeftJoin<PropertyTypeGroupDto>().On<PropertyTypeDto, PropertyTypeGroupDto>((pt, ptg) => pt.PropertyTypeGroupId == ptg.Id)
                .LeftJoin<MemberPropertyTypeDto>().On<PropertyTypeDto, MemberPropertyTypeDto>((pt, mpt) => pt.Id == mpt.PropertyTypeId)
                .OrderBy<ContentTypeDto>(x => x.NodeId)
                .AndBy<PropertyTypeGroupDto>(x => x.SortOrder, x => x.Id) // NULLs will come first or last, never mind, we deal with it below
                .AndBy<PropertyTypeDto>(x => x.SortOrder, x => x.Id);

            var propertyDtos = Database.Fetch<PropertyTypeCommonDto>(sql2);
            var builtinProperties = Constants.Conventions.Member.GetStandardPropertyTypeStubs();

            var groupIx = 0;
            var propertyIx = 0;
            foreach (var contentType in contentTypes.Values)
            {
                // only IContentType is publishing
                var isPublishing = contentType is IContentType;

                // get group-less properties (in case NULL is ordered first)
                var noGroupPropertyTypes = new PropertyTypeCollection(isPublishing);
                while (propertyIx < propertyDtos.Count && propertyDtos[propertyIx].ContentTypeId == contentType.Id && propertyDtos[propertyIx].PropertyTypeGroupId == null)
                {
                    noGroupPropertyTypes.Add(MapPropertyType(contentType, propertyDtos[propertyIx], builtinProperties));
                    propertyIx++;
                }

                // get groups and their properties
                var groupCollection = new PropertyGroupCollection();
                while (groupIx < groupDtos.Count && groupDtos[groupIx].ContentTypeNodeId == contentType.Id)
                {
                    var group = MapPropertyGroup(groupDtos[groupIx], isPublishing);
                    groupCollection.Add(group);
                    groupIx++;

                    while (propertyIx < propertyDtos.Count && propertyDtos[propertyIx].ContentTypeId == contentType.Id && propertyDtos[propertyIx].PropertyTypeGroupId == group.Id)
                    {
                        group.PropertyTypes.Add(MapPropertyType(contentType, propertyDtos[propertyIx], builtinProperties));
                        propertyIx++;
                    }
                }
                contentType.PropertyGroups = groupCollection;

                // get group-less properties (in case NULL is ordered last)
                while (propertyIx < propertyDtos.Count && propertyDtos[propertyIx].ContentTypeId == contentType.Id && propertyDtos[propertyIx].PropertyTypeGroupId == null)
                {
                    noGroupPropertyTypes.Add(MapPropertyType(contentType, propertyDtos[propertyIx], builtinProperties));
                    propertyIx++;
                }
                contentType.NoGroupPropertyTypes = noGroupPropertyTypes;

                // ensure builtin properties
                if (contentType is MemberType memberType)
                {
                    // ensure that property types exist (ok if they already exist)
                    foreach (var (alias, propertyType) in builtinProperties)
                    {
                        var added = memberType.AddPropertyType(propertyType, Constants.Conventions.Member.StandardPropertiesGroupAlias, Constants.Conventions.Member.StandardPropertiesGroupName);
                        if (added)
                        {
                            var access = new MemberTypePropertyProfileAccess(false, false, false);
                            memberType.MemberTypePropertyTypes[alias] = access;
                        }
                    }
                }
            }
        }

        private PropertyGroup MapPropertyGroup(PropertyTypeGroupDto dto, bool isPublishing)
        {
            return new PropertyGroup(new PropertyTypeCollection(isPublishing))
            {
                Id = dto.Id,
                Key = dto.UniqueId,
                Type = (PropertyGroupType)dto.Type,
                Name = dto.Text,
                Alias = dto.Alias,
                SortOrder = dto.SortOrder
            };
        }

        private PropertyType MapPropertyType(IContentTypeComposition contentType, PropertyTypeCommonDto dto, IDictionary<string, PropertyType> builtinProperties)
        {
            var groupId = dto.PropertyTypeGroupId;

            var readonlyStorageType = builtinProperties.TryGetValue(dto.Alias, out var propertyType);
            var storageType = readonlyStorageType ? propertyType.ValueStorageType : Enum<ValueStorageType>.Parse(dto.DataTypeDto.DbType);

            if (contentType is MemberType memberType)
            {
                var access = new MemberTypePropertyProfileAccess(dto.ViewOnProfile, dto.CanEdit, dto.IsSensitive);
                memberType.MemberTypePropertyTypes[dto.Alias] = access;
            }

            return new PropertyType(dto.DataTypeDto.EditorAlias, storageType, readonlyStorageType, dto.Alias)
            {
                Description = dto.Description,
                DataTypeId = dto.DataTypeId,
                DataTypeKey = dto.DataTypeDto.NodeDto.UniqueId,
                Id = dto.Id,
                Key = dto.UniqueId,
                Mandatory = dto.Mandatory,
                MandatoryMessage = dto.MandatoryMessage,
                Name = dto.Name,
                PropertyGroupId = groupId.HasValue ? new Lazy<int>(() => groupId.Value) : null,
                SortOrder = dto.SortOrder,
                ValidationRegExp = dto.ValidationRegExp,
                ValidationRegExpMessage = dto.ValidationRegExpMessage,
                Variations = (ContentVariation)dto.Variations,
                LabelOnTop = dto.LabelOnTop
            };
        }
    }
}

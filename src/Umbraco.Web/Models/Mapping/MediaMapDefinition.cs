﻿using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;
using Umbraco.Core.Configuration.UmbracoSettings;
using System;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for media.
    /// </summary>
    internal class MediaMapDefinition : IMapDefinition
    {
        private readonly CommonMapper _commonMapper;
        private readonly ILogger _logger;
        private readonly IMediaService _mediaService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly PropertyEditorCollection _propertyEditorCollection;
        private readonly TabsAndPropertiesMapper<IMedia> _tabsAndPropertiesMapper;
        private readonly IUmbracoSettingsSection _umbracoSettingsSection;

        public MediaMapDefinition(ICultureDictionary cultureDictionary, ILogger logger, CommonMapper commonMapper, IMediaService mediaService, IMediaTypeService mediaTypeService,
            ILocalizedTextService localizedTextService, PropertyEditorCollection propertyEditorCollection, IUmbracoSettingsSection umbracoSettingsSection, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
        {
            _logger = logger;
            _commonMapper = commonMapper;
            _mediaService = mediaService;
            _mediaTypeService = mediaTypeService;
            _propertyEditorCollection = propertyEditorCollection;
            _umbracoSettingsSection = umbracoSettingsSection ?? throw new ArgumentNullException(nameof(umbracoSettingsSection));

            _tabsAndPropertiesMapper = new TabsAndPropertiesMapper<IMedia>(cultureDictionary, localizedTextService, contentTypeBaseServiceProvider);
        }

        public void DefineMaps(UmbracoMapper mapper)
        {
            mapper.Define<IMedia, ContentPropertyCollectionDto>((source, context) => new ContentPropertyCollectionDto(), Map);
            mapper.Define<IMedia, MediaItemDisplay>((source, context) => new MediaItemDisplay(), Map);
            mapper.Define<IMedia, ContentItemBasic<ContentPropertyBasic>>((source, context) => new ContentItemBasic<ContentPropertyBasic>(), Map);
        }

        // Umbraco.Code.MapAll
        private static void Map(IMedia source, ContentPropertyCollectionDto target, MapperContext context)
        {
            target.Properties = context.MapEnumerable<IProperty, ContentPropertyDto>(source.Properties);
        }

        // Umbraco.Code.MapAll -Properties -Errors -Edited -Updater -Alias -IsContainer
        private void Map(IMedia source, MediaItemDisplay target, MapperContext context)
        {
            target.ContentApps = _commonMapper.GetContentApps(source);
            target.ContentType = _commonMapper.GetContentType(source, context);
            target.ContentTypeId = source.ContentType.Id;
            target.ContentTypeAlias = source.ContentType.Alias;
            target.ContentTypeName = source.ContentType.Name;
            target.CreateDate = source.CreateDate;
            target.Icon = source.ContentType.Icon;
            target.Id = source.Id;
            target.IsChildOfListView = DetermineIsChildOfListView(source);
            target.Key = source.Key;
            target.MediaLink = string.Join(",", source.GetUrls(_umbracoSettingsSection.Content, _logger, _propertyEditorCollection));
            target.Name = source.Name;
            target.Owner = _commonMapper.GetOwner(source, context);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.SortOrder = source.SortOrder;
            target.State = null;
            target.Tabs = _tabsAndPropertiesMapper.Map(source, context);
            target.Trashed = source.Trashed;
            target.TreeNodeUrl = _commonMapper.GetTreeNodeUrl<MediaTreeController>(source);
            target.Udi = Udi.Create(Constants.UdiEntityType.Media, source.Key);
            target.UpdateDate = source.UpdateDate;
            target.VariesByCulture = source.ContentType.VariesByCulture();
        }

        // Umbraco.Code.MapAll -Edited -Updater -Alias
        private void Map(IMedia source, ContentItemBasic<ContentPropertyBasic> target, MapperContext context)
        {
            target.ContentTypeId = source.ContentType.Id;
            target.ContentTypeAlias = source.ContentType.Alias;
            target.CreateDate = source.CreateDate;
            target.Icon = source.ContentType.Icon;
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.Owner = _commonMapper.GetOwner(source, context);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Properties = context.MapEnumerable<IProperty, ContentPropertyBasic>(source.Properties);
            target.SortOrder = source.SortOrder;
            target.State = null;
            target.Trashed = source.Trashed;
            target.Udi = Udi.Create(Constants.UdiEntityType.Media, source.Key);
            target.UpdateDate = source.UpdateDate;
            target.VariesByCulture = source.ContentType.VariesByCulture();
        }

        private bool DetermineIsChildOfListView(IMedia source)
        {
            // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
            var parent = _mediaService.GetParent(source);
            return parent != null && (parent.ContentType.IsContainer || _mediaTypeService.HasContainerInPath(parent.Path));
        }
    }
}

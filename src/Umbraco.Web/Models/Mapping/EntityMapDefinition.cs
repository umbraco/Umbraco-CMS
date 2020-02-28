﻿using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Examine;

namespace Umbraco.Web.Models.Mapping
{
    internal class EntityMapDefinition : IMapDefinition
    {
        public void DefineMaps(UmbracoMapper mapper)
        {
            mapper.Define<IEntitySlim, EntityBasic>((source, context) => new EntityBasic(), Map);
            mapper.Define<PropertyType, EntityBasic>((source, context) => new EntityBasic(), Map);
            mapper.Define<PropertyGroup, EntityBasic>((source, context) => new EntityBasic(), Map);
            mapper.Define<IUser, EntityBasic>((source, context) => new EntityBasic(), Map);
            mapper.Define<ITemplate, EntityBasic>((source, context) => new EntityBasic(), Map);
            mapper.Define<EntityBasic, ContentTypeSort>((source, context) => new ContentTypeSort(), Map);
            mapper.Define<IContentTypeComposition, EntityBasic>((source, context) => new EntityBasic(), Map);
            mapper.Define<IEntitySlim, SearchResultEntity>((source, context) => new SearchResultEntity(), Map);
            mapper.Define<ISearchResult, SearchResultEntity>((source, context) => new SearchResultEntity(), Map);
            mapper.Define<ISearchResults, IEnumerable<SearchResultEntity>>((source, context) => context.MapEnumerable<ISearchResult, SearchResultEntity>(source));
            mapper.Define<IEnumerable<ISearchResult>, IEnumerable<SearchResultEntity>>((source, context) => context.MapEnumerable<ISearchResult, SearchResultEntity>(source));
        }

        // Umbraco.Code.MapAll -Alias
        private static void Map(IEntitySlim source, EntityBasic target, MapperContext context)
        {
            target.Icon = MapContentTypeIcon(source);
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = MapName(source, context);
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Trashed = source.Trashed;
            target.Udi = Udi.Create(ObjectTypes.GetUdiType(source.NodeObjectType), source.Key);

            if (source is IContentEntitySlim contentSlim)
            {
                source.AdditionalData["ContentTypeAlias"] = contentSlim.ContentTypeAlias;
            }

            if (source is IDocumentEntitySlim documentSlim)
            {
                source.AdditionalData["IsPublished"] = documentSlim.Published;
            }

            if (source is IMediaEntitySlim mediaSlim)
            {
                source.AdditionalData["MediaPath"] = mediaSlim.MediaPath;
            }

            // NOTE: we're mapping the objects in AdditionalData by object reference here.
            // it works fine for now, but it's something to keep in mind in the future
            foreach(var kvp in source.AdditionalData)
            {
                target.AdditionalData[kvp.Key] = kvp.Value;
            }

            target.AdditionalData.Add("IsContainer", source.IsContainer);
        }

        // Umbraco.Code.MapAll -Udi -Trashed
        private static void Map(PropertyType source, EntityBasic target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Icon = "icon-box";
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = -1;
            target.Path = "";
        }

        // Umbraco.Code.MapAll -Udi -Trashed
        private static void Map(PropertyGroup source, EntityBasic target, MapperContext context)
        {
            target.Alias = source.Name.ToLowerInvariant();
            target.Icon = "icon-tab";
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = -1;
            target.Path = "";
        }

        // Umbraco.Code.MapAll -Udi -Trashed
        private static void Map(IUser source, EntityBasic target, MapperContext context)
        {
            target.Alias = source.Username;
            target.Icon = Constants.Icons.User;
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = -1;
            target.Path = "";
        }

        // Umbraco.Code.MapAll -Trashed
        private static void Map(ITemplate source, EntityBasic target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Icon = Constants.Icons.Template;
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = -1;
            target.Path = source.Path;
            target.Udi = Udi.Create(Constants.UdiEntityType.Template, source.Key);
        }

        // Umbraco.Code.MapAll -SortOrder
        private static void Map(EntityBasic source, ContentTypeSort target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Id = new Lazy<int>(() => Convert.ToInt32(source.Id));
        }

        // Umbraco.Code.MapAll -Trashed
        private static void Map(IContentTypeComposition source, EntityBasic target, MapperContext context)
        {
            target.Alias = source.Alias;
            target.Icon = source.Icon;
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Udi = ContentTypeMapDefinition.MapContentTypeUdi(source);
        }

        // Umbraco.Code.MapAll -Trashed -Alias -Score
        private static void Map(EntitySlim source, SearchResultEntity target, MapperContext context)
        {
            target.Icon = MapContentTypeIcon(source);
            target.Id = source.Id;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentId = source.ParentId;
            target.Path = source.Path;
            target.Udi = Udi.Create(ObjectTypes.GetUdiType(source.NodeObjectType), source.Key);

            if (target.Icon.IsNullOrWhiteSpace())
            {
                if (source.NodeObjectType == Constants.ObjectTypes.Member)
                    target.Icon = Constants.Icons.Member;
                else if (source.NodeObjectType == Constants.ObjectTypes.DataType)
                    target.Icon = Constants.Icons.DataType;
                else if (source.NodeObjectType == Constants.ObjectTypes.DocumentType)
                    target.Icon = Constants.Icons.ContentType;
                else if (source.NodeObjectType == Constants.ObjectTypes.MediaType)
                    target.Icon = Constants.Icons.MediaType;
                else if (source.NodeObjectType == Constants.ObjectTypes.TemplateType)
                    target.Icon = Constants.Icons.Template;
            }
        }

        // Umbraco.Code.MapAll -Alias -Trashed
        private static void Map(ISearchResult source, SearchResultEntity target, MapperContext context)
        {
            target.Id = source.Id;
            target.Score = source.Score;

            // TODO: Properly map this (not aftermap)

            //get the icon if there is one
            target.Icon = source.Values.ContainsKey(UmbracoExamineIndex.IconFieldName)
                ? source.Values[UmbracoExamineIndex.IconFieldName]
                : Constants.Icons.DefaultIcon;

            target.Name = source.Values.ContainsKey("nodeName") ? source.Values["nodeName"] : "[no name]";

            if (source.Values.TryGetValue(UmbracoExamineIndex.UmbracoFileFieldName, out var umbracoFile))
            {
                if (umbracoFile != null)
                {
                    target.Name = $"{target.Name} ({umbracoFile})";
                }
            }

            if (source.Values.ContainsKey(UmbracoExamineIndex.NodeKeyFieldName))
            {
                if (Guid.TryParse(source.Values[UmbracoExamineIndex.NodeKeyFieldName], out var key))
                {
                    target.Key = key;

                    //need to set the UDI
                    if (source.Values.ContainsKey(LuceneIndex.CategoryFieldName))
                    {
                        switch (source.Values[LuceneIndex.CategoryFieldName])
                        {
                            case IndexTypes.Member:
                                target.Udi = new GuidUdi(Constants.UdiEntityType.Member, target.Key);
                                break;
                            case IndexTypes.Content:
                                target.Udi = new GuidUdi(Constants.UdiEntityType.Document, target.Key);
                                break;
                            case IndexTypes.Media:
                                target.Udi = new GuidUdi(Constants.UdiEntityType.Media, target.Key);
                                break;
                        }
                    }
                }
            }

            if (source.Values.ContainsKey("parentID"))
            {
                if (int.TryParse(source.Values["parentID"], out var parentId))
                {
                    target.ParentId = parentId;
                }
                else
                {
                    target.ParentId = -1;
                }
            }

            target.Path = source.Values.ContainsKey(UmbracoExamineIndex.IndexPathFieldName) ? source.Values[UmbracoExamineIndex.IndexPathFieldName] : "";

            if (source.Values.ContainsKey(LuceneIndex.ItemTypeFieldName))
            {
                target.AdditionalData.Add("contentType", source.Values[LuceneIndex.ItemTypeFieldName]);
            }
        }

        private static string MapContentTypeIcon(IEntitySlim entity)
        {
            switch (entity)
            {
                case IMemberEntitySlim memberEntity:
                    return memberEntity.ContentTypeIcon.IfNullOrWhiteSpace(Constants.Icons.Member);
                case IContentEntitySlim contentEntity:
                    // NOTE: this case covers both content and media entities
                    return contentEntity.ContentTypeIcon;                
            }

            return null;
        }

        private static string MapName(IEntitySlim source, MapperContext context)
        {
            if (!(source is DocumentEntitySlim doc))
                return source.Name;

            // invariant = only 1 name
            if (!doc.Variations.VariesByCulture()) return source.Name;

            // variant = depends on culture
            var culture = context.GetCulture();

            // if there's no culture here, the issue is somewhere else (UI, whatever) - throw!
            if (culture == null)
                //throw new InvalidOperationException("Missing culture in mapping options.");
                // TODO: we should throw, but this is used in various places that won't set a culture yet
                return source.Name;

            // if we don't have a name for a culture, it means the culture is not available, and
            // hey we should probably not be mapping it, but it's too late, return a fallback name
            return doc.CultureNames.TryGetValue(culture, out var name) && !name.IsNullOrWhiteSpace() ? name : $"({source.Name})";
        }
    }
}

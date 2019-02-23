using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Examine;

namespace Umbraco.Web.Models.Mapping
{
    internal class EntityMapperProfile : Profile
    {
        private static string GetContentTypeIcon(IEntitySlim entity)
            => entity is ContentEntitySlim contentEntity ? contentEntity.ContentTypeIcon : null;

        public EntityMapperProfile()
        {
            // create, capture, cache
            var contentTypeUdiResolver = new ContentTypeUdiResolver();

            CreateMap<IEntitySlim, EntityBasic>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<NameResolver>())
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(ObjectTypes.GetUdiType(src.NodeObjectType), src.Key)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => GetContentTypeIcon(src)))
                .ForMember(dest => dest.Trashed, opt => opt.MapFrom(src => src.Trashed))
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src.NodeObjectType == Constants.ObjectTypes.Member && dest.Icon.IsNullOrWhiteSpace())
                    {
                        dest.Icon = "icon-user";
                    }

                    dest.AdditionalData.Add("IsContainer", src.IsContainer);
                });

            CreateMap<PropertyType, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(_ => "icon-box"))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(_ => ""))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(_ => -1))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<PropertyGroup, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(_ => "icon-tab"))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(_ => ""))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(_ => -1))
                //in v6 the 'alias' is it's lower cased name so we'll stick to that.
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Name.ToLowerInvariant()))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IUser, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(_ => "icon-user"))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(_ => ""))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(_ => -1))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<ITemplate, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.Template, src.Key)))
               .ForMember(dest => dest.Icon, opt => opt.MapFrom(_ => "icon-layout"))
               .ForMember(dest => dest.Path, opt => opt.MapFrom(src => src.Path))
               .ForMember(dest => dest.ParentId, opt => opt.MapFrom(_ => -1))
               .ForMember(dest => dest.Trashed, opt => opt.Ignore())
               .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<EntityBasic, ContentTypeSort>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => new Lazy<int>(() => Convert.ToInt32(src.Id))))
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore());

            CreateMap<IContentTypeComposition, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => contentTypeUdiResolver.Resolve(src)))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(src => src.Path))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<EntitySlim, SearchResultEntity>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(ObjectTypes.GetUdiType(src.NodeObjectType), src.Key)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src=> GetContentTypeIcon(src)))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Score, opt => opt.Ignore())
                .AfterMap((entity, basic) =>
                {
                    if (basic.Icon.IsNullOrWhiteSpace())
                    {
                        if (entity.NodeObjectType == Constants.ObjectTypes.Member)
                            basic.Icon = "icon-user";
                        else if (entity.NodeObjectType == Constants.ObjectTypes.DataType)
                            basic.Icon = "icon-autofill";
                        else if (entity.NodeObjectType == Constants.ObjectTypes.DocumentType)
                            basic.Icon = "icon-item-arrangement";
                        else if (entity.NodeObjectType == Constants.ObjectTypes.MediaType)
                            basic.Icon = "icon-thumbnails";
                        else if (entity.NodeObjectType == Constants.ObjectTypes.TemplateType)
                            basic.Icon = "icon-newspaper-alt";
                    }
                });

            CreateMap<ISearchResult, SearchResultEntity>()
                //default to document icon
                  .ForMember(dest => dest.Score, opt => opt.MapFrom(result => result.Score))
                  .ForMember(dest => dest.Udi, opt => opt.Ignore())
                  .ForMember(dest => dest.Icon, opt => opt.Ignore())
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.Name, opt => opt.Ignore())
                  .ForMember(dest => dest.Key, opt => opt.Ignore())
                  .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                  .ForMember(dest => dest.Alias, opt => opt.Ignore())
                  .ForMember(dest => dest.Path, opt => opt.Ignore())
                  .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                  .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                  .AfterMap((src, dest) =>
                      {
                          // TODO: Properly map this (not aftermap)

                          //get the icon if there is one
                          dest.Icon = src.Values.ContainsKey(UmbracoExamineIndex.IconFieldName)
                              ? src.Values[UmbracoExamineIndex.IconFieldName]
                              : "icon-document";

                          dest.Name = src.Values.ContainsKey("nodeName") ? src.Values["nodeName"] : "[no name]";
                          if (src.Values.ContainsKey(UmbracoExamineIndex.NodeKeyFieldName))
                          {
                              Guid key;
                              if (Guid.TryParse(src.Values[UmbracoExamineIndex.NodeKeyFieldName], out key))
                              {
                                  dest.Key = key;

                                  //need to set the UDI
                                  if (src.Values.ContainsKey(LuceneIndex.CategoryFieldName))
                                  {
                                      switch (src.Values[LuceneIndex.CategoryFieldName])
                                      {
                                          case IndexTypes.Member:
                                              dest.Udi = new GuidUdi(Constants.UdiEntityType.Member, dest.Key);
                                              break;
                                          case IndexTypes.Content:
                                              dest.Udi = new GuidUdi(Constants.UdiEntityType.Document, dest.Key);
                                              break;
                                          case IndexTypes.Media:
                                              dest.Udi = new GuidUdi(Constants.UdiEntityType.Media, dest.Key);
                                              break;
                                      }
                                  }
                              }
                          }

                          if (src.Values.ContainsKey("parentID"))
                          {
                              int parentId;
                              if (int.TryParse(src.Values["parentID"], out parentId))
                              {
                                  dest.ParentId = parentId;
                              }
                              else
                              {
                                  dest.ParentId = -1;
                              }
                          }
                          dest.Path = src.Values.ContainsKey(UmbracoExamineIndex.IndexPathFieldName) ? src.Values[UmbracoExamineIndex.IndexPathFieldName] : "";

                          if (src.Values.ContainsKey(LuceneIndex.ItemTypeFieldName))
                          {
                              dest.AdditionalData.Add("contentType", src.Values[LuceneIndex.ItemTypeFieldName]);
                          }
                      });

            CreateMap<ISearchResults, IEnumerable<SearchResultEntity>>()
                  .ConvertUsing(results => results.Select(Mapper.Map<SearchResultEntity>).ToList());

            CreateMap<IEnumerable<ISearchResult>, IEnumerable<SearchResultEntity>>()
                  .ConvertUsing(results => results.Select(Mapper.Map<SearchResultEntity>).ToList());
        }

        /// <summary>
        /// Resolves the name for a content item/content variant
        /// </summary>
        private class NameResolver : IValueResolver<IEntitySlim, EntityBasic, string>
        {
            public string Resolve(IEntitySlim source, EntityBasic destination, string destMember, ResolutionContext context)
            {
                if (!(source is DocumentEntitySlim doc))
                    return source.Name;

                // invariant = only 1 name
                if (!doc.Variations.VariesByCulture()) return source.Name;

                // variant = depends on culture
                var culture = context.Options.GetCulture();

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
}

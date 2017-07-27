using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Examine;

namespace Umbraco.Web.Models.Mapping
{
    internal class EntityProfile : Profile
    {
        public EntityProfile()
        {
            // create, capture, cache
            var contentTypeUdiResolver = new ContentTypeUdiResolver();

            CreateMap<UmbracoEntity, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(UmbracoObjectTypesExtensions.GetUdiType(src.NodeObjectTypeId), src.Key)))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.ContentTypeIcon))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src.NodeObjectTypeId == Constants.ObjectTypes.MemberGuid && dest.Icon.IsNullOrWhiteSpace())
                    {
                        dest.Icon = "icon-user";
                    }
                });

            CreateMap<PropertyType, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.UseValue("icon-box"))
                .ForMember(dest => dest.Path, opt => opt.UseValue(""))
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<PropertyGroup, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.UseValue("icon-tab"))
                .ForMember(dest => dest.Path, opt => opt.UseValue(""))
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                //in v6 the 'alias' is it's lower cased name so we'll stick to that.
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Name.ToLowerInvariant()))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IUser, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.UseValue("icon-user"))
                .ForMember(dest => dest.Path, opt => opt.UseValue(""))
                .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<ITemplate, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.Template, src.Key)))
               .ForMember(dest => dest.Icon, opt => opt.UseValue("icon-layout"))
               .ForMember(dest => dest.Path, opt => opt.MapFrom(src => src.Path))
               .ForMember(dest => dest.ParentId, opt => opt.UseValue(-1))
               .ForMember(dest => dest.Trashed, opt => opt.Ignore())
               .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            //CreateMap<EntityBasic, ITemplate>()
            //    .ConstructUsing(basic => new Template(basic.Name, basic.Alias)
            //    {
            //        Id = Convert.ToInt32(basic.Id),
            //        Key = basic.Key
            //    })
            //   .ForMember(t => t.Path, opt => opt.Ignore())
            //   .ForMember(t => t.Id, opt => opt.MapFrom(template => Convert.ToInt32(template.Id)))
            //   .ForMember(dest => dest.VirtualPath, opt => opt.Ignore())
            //   .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
            //   .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
            //   .ForMember(dest => dest.Content, opt => opt.Ignore());

            CreateMap<EntityBasic, ContentTypeSort>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => new Lazy<int>(() => Convert.ToInt32(src.Id))))
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore());

            CreateMap<IContentTypeComposition, EntityBasic>()
                .ForMember(dest => dest.Udi, opt => opt.ResolveUsing(src => contentTypeUdiResolver.Resolve(src)))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(src => src.Path))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<SearchResult, EntityBasic>()
                //default to document icon
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
                          //get the icon if there is one
                          dest.Icon = src.Fields.ContainsKey(BaseUmbracoIndexer.IconFieldName)
                              ? src.Fields[BaseUmbracoIndexer.IconFieldName]
                              : "icon-document";

                          dest.Name = src.Fields.ContainsKey("nodeName") ? src.Fields["nodeName"] : "[no name]";
                          if (src.Fields.ContainsKey(UmbracoContentIndexer.NodeKeyFieldName))
                          {
                              Guid key;
                              if (Guid.TryParse(src.Fields[UmbracoContentIndexer.NodeKeyFieldName], out key))
                              {
                                  dest.Key = key;

                                  //need to set the UDI
                                  if (src.Fields.ContainsKey(LuceneIndexer.IndexTypeFieldName))
                                  {
                                      switch (src.Fields[LuceneIndexer.IndexTypeFieldName])
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

                          if (src.Fields.ContainsKey("parentID"))
                          {
                              int parentId;
                              if (int.TryParse(src.Fields["parentID"], out parentId))
                              {
                                  dest.ParentId = parentId;
                              }
                              else
                              {
                                  dest.ParentId = -1;
                              }
                          }
                          dest.Path = src.Fields.ContainsKey(UmbracoContentIndexer.IndexPathFieldName) ? src.Fields[UmbracoContentIndexer.IndexPathFieldName] : "";

                          if (src.Fields.ContainsKey(LuceneIndexer.NodeTypeAliasFieldName))
                          {
                              dest.AdditionalData.Add("contentType", src.Fields[LuceneIndexer.NodeTypeAliasFieldName]);
                          }
                      });

            CreateMap<ILuceneSearchResults, IEnumerable<EntityBasic>>()
                  .ConvertUsing(results => results.Select(Mapper.Map<EntityBasic>).ToList());

            CreateMap<IEnumerable<SearchResult>, IEnumerable<EntityBasic>>()
                  .ConvertUsing(results => results.Select(Mapper.Map<EntityBasic>).ToList());
        }
    }
}

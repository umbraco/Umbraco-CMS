using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using UmbracoExamine;

namespace Umbraco.Web.Models.Mapping
{
    internal class EntityModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<UmbracoEntity, EntityBasic>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(x => Udi.Create(UmbracoObjectTypesExtensions.GetUdiType(x.NodeObjectTypeId), x.Key))) 
                .ForMember(basic => basic.Icon, expression => expression.MapFrom(entity => entity.ContentTypeIcon))
                .ForMember(dto => dto.Trashed, expression => expression.MapFrom(x => x.Trashed))
                .ForMember(x => x.Alias, expression => expression.Ignore())
                .AfterMap((entity, basic) =>
                {
                    if (entity.NodeObjectTypeId == Constants.ObjectTypes.MemberGuid && basic.Icon.IsNullOrWhiteSpace())
                    {
                        basic.Icon = "icon-user";
                    }
                });            

            config.CreateMap<PropertyType, EntityBasic>()
                .ForMember(x => x.Udi, expression => expression.Ignore())
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-box"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<PropertyGroup, EntityBasic>()
                .ForMember(x => x.Udi, expression => expression.Ignore())
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-tab"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                //in v6 the 'alias' is it's lower cased name so we'll stick to that.
                .ForMember(basic => basic.Alias, expression => expression.MapFrom(group => group.Name.ToLowerInvariant()))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<IUser, EntityBasic>()
                .ForMember(x => x.Udi, expression => expression.Ignore())
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-user"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                .ForMember(basic => basic.Alias, expression => expression.MapFrom(user => user.Username))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<ITemplate, EntityBasic>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(x => Udi.Create(Constants.UdiEntityType.Template, x.Key)))
               .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-layout"))
               .ForMember(basic => basic.Path, expression => expression.MapFrom(template => template.Path))
               .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
               .ForMember(dto => dto.Trashed, expression => expression.Ignore())
               .ForMember(x => x.AdditionalData, expression => expression.Ignore());
            
            config.CreateMap<EntityBasic, ContentTypeSort>()
                .ForMember(x => x.Id, expression => expression.MapFrom(entity => new Lazy<int>(() => Convert.ToInt32(entity.Id))))
                .ForMember(x => x.SortOrder, expression => expression.Ignore());

            config.CreateMap<IContentTypeComposition, EntityBasic>()
                .ForMember(x => x.Udi, expression => expression.ResolveUsing(new ContentTypeUdiResolver()))
                .ForMember(basic => basic.Path, expression => expression.MapFrom(x => x.Path))
                .ForMember(basic => basic.ParentId, expression => expression.MapFrom(x => x.ParentId))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<UmbracoEntity, SearchResultItem>()
                .ForMember(x => x.Udi, expression => expression.MapFrom(x => Udi.Create(UmbracoObjectTypesExtensions.GetUdiType(x.NodeObjectTypeId), x.Key)))
                .ForMember(basic => basic.Icon, expression => expression.MapFrom(entity => entity.ContentTypeIcon))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore())
                .ForMember(x => x.Score, expression => expression.Ignore())
                .AfterMap((entity, basic) =>
                {
                    if (basic.Icon.IsNullOrWhiteSpace())
                    {
                        if (entity.NodeObjectTypeId == Constants.ObjectTypes.MemberGuid)
                            basic.Icon = "icon-user";
                        else if (entity.NodeObjectTypeId == Constants.ObjectTypes.DataTypeGuid) 
                            basic.Icon = "icon-autofill";
                        else if (entity.NodeObjectTypeId == Constants.ObjectTypes.DocumentTypeGuid)
                            basic.Icon = "icon-item-arrangement";
                        else if (entity.NodeObjectTypeId == Constants.ObjectTypes.MediaTypeGuid)
                            basic.Icon = "icon-thumbnails";
                        else if (entity.NodeObjectTypeId == Constants.ObjectTypes.TemplateTypeGuid)
                            basic.Icon = "icon-newspaper-alt";
                    }
                });

            config.CreateMap<SearchResult, SearchResultItem>()
                //default to document icon
                  .ForMember(x => x.Score, expression => expression.MapFrom(result => result.Score))
                  .ForMember(x => x.Udi, expression => expression.Ignore())
                  .ForMember(x => x.Icon, expression => expression.Ignore())
                  .ForMember(x => x.Id, expression => expression.MapFrom(result => result.Id))
                  .ForMember(x => x.Name, expression => expression.Ignore())
                  .ForMember(x => x.Key, expression => expression.Ignore())
                  .ForMember(x => x.ParentId, expression => expression.Ignore())
                  .ForMember(x => x.Alias, expression => expression.Ignore())
                  .ForMember(x => x.Path, expression => expression.Ignore())
                  .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                  .ForMember(x => x.AdditionalData, expression => expression.Ignore())
                  .AfterMap((result, basic) =>
                      {   

                          //get the icon if there is one
                          basic.Icon = result.Fields.ContainsKey(UmbracoContentIndexer.IconFieldName) 
                              ? result.Fields[UmbracoContentIndexer.IconFieldName] 
                              : "icon-document";

                          basic.Name = result.Fields.ContainsKey("nodeName") ? result.Fields["nodeName"] : "[no name]";
                          if (result.Fields.ContainsKey(UmbracoContentIndexer.NodeKeyFieldName))
                          {
                              Guid key;
                              if (Guid.TryParse(result.Fields[UmbracoContentIndexer.NodeKeyFieldName], out key))
                              {
                                  basic.Key = key;

                                  //need to set the UDI
                                  if (result.Fields.ContainsKey(LuceneIndexer.IndexTypeFieldName))
                                  {
                                      switch (result.Fields[LuceneIndexer.IndexTypeFieldName])
                                      {
                                          case IndexTypes.Member:
                                              basic.Udi = new GuidUdi(Constants.UdiEntityType.Member, basic.Key);
                                              break;
                                          case IndexTypes.Content:
                                              basic.Udi = new GuidUdi(Constants.UdiEntityType.Document, basic.Key);
                                              break;
                                          case IndexTypes.Media:
                                              basic.Udi = new GuidUdi(Constants.UdiEntityType.Media, basic.Key);
                                              break;
                                      }
                                  }
                              }
                          }

                          if (result.Fields.ContainsKey("parentID"))
                          {
                              int parentId;
                              if (int.TryParse(result.Fields["parentID"], out parentId))
                              {
                                  basic.ParentId = parentId;
                              }
                              else
                              {
                                  basic.ParentId = -1;
                              }
                          }
                          basic.Path = result.Fields.ContainsKey(UmbracoContentIndexer.IndexPathFieldName) ? result.Fields[UmbracoContentIndexer.IndexPathFieldName] : "";
                          
                          if (result.Fields.ContainsKey(UmbracoContentIndexer.NodeTypeAliasFieldName))
                          {
                              basic.AdditionalData.Add("contentType", result.Fields[UmbracoContentIndexer.NodeTypeAliasFieldName]);
                          }
                      });

            config.CreateMap<ISearchResults, IEnumerable<SearchResultItem>>()
                  .ConvertUsing(results => results.Select(Mapper.Map<SearchResultItem>).ToList());

            config.CreateMap<IEnumerable<SearchResult>, IEnumerable<SearchResultItem>>()
                  .ConvertUsing(results => results.Select(Mapper.Map<SearchResultItem>).ToList());
        }
    }
}
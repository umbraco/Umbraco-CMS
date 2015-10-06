using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Examine;
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
                .ForMember(basic => basic.Icon, expression => expression.MapFrom(entity => entity.ContentTypeIcon))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore());

            config.CreateMap<PropertyType, EntityBasic>()
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-box"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<PropertyGroup, EntityBasic>()
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-tab"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                //in v6 the 'alias' is it's lower cased name so we'll stick to that.
                .ForMember(basic => basic.Alias, expression => expression.MapFrom(group => group.Name.ToLowerInvariant()))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<IUser, EntityBasic>()
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-user"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                .ForMember(basic => basic.Alias, expression => expression.MapFrom(user => user.Username))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<ITemplate, EntityBasic>()
               .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-layout"))
               .ForMember(basic => basic.Path, expression => expression.MapFrom(template => template.Path))
               .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
               .ForMember(dto => dto.Trashed, expression => expression.Ignore())
               .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            //config.CreateMap<EntityBasic, ITemplate>()
            //    .ConstructUsing(basic => new Template(basic.Name, basic.Alias)
            //    {
            //        Id = Convert.ToInt32(basic.Id),
            //        Key = basic.Key
            //    })
            //   .ForMember(t => t.Path, expression => expression.Ignore())
            //   .ForMember(t => t.Id, expression => expression.MapFrom(template => Convert.ToInt32(template.Id)))
            //   .ForMember(x => x.VirtualPath, expression => expression.Ignore())
            //   .ForMember(x => x.CreateDate, expression => expression.Ignore())
            //   .ForMember(x => x.UpdateDate, expression => expression.Ignore())
            //   .ForMember(x => x.Content, expression => expression.Ignore());

            config.CreateMap<EntityBasic, ContentTypeSort>()
                .ForMember(x => x.Id, expression => expression.MapFrom(entity => new Lazy<int>(() => Convert.ToInt32(entity.Id))))
                .ForMember(x => x.SortOrder, expression => expression.Ignore());

            config.CreateMap<IContentTypeComposition, EntityBasic>()
                .ForMember(basic => basic.Path, expression => expression.MapFrom(x => x.Path))
                .ForMember(basic => basic.ParentId, expression => expression.MapFrom(x => x.ParentId))
                .ForMember(dto => dto.Trashed, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<SearchResult, EntityBasic>()
                //default to document icon
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
                          if (result.Fields.ContainsKey("__NodeKey"))
                          {
                              Guid key;
                              if (Guid.TryParse(result.Fields["__NodeKey"], out key))
                              {
                                  basic.Key = key;
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
                          basic.Path = result.Fields.ContainsKey("__Path") ? result.Fields["__Path"] : "";
                          
                          if (result.Fields.ContainsKey(UmbracoContentIndexer.NodeTypeAliasFieldName))
                          {
                              basic.AdditionalData.Add("contentType", result.Fields[UmbracoContentIndexer.NodeTypeAliasFieldName]);
                          }
                      });

            config.CreateMap<ISearchResults, IEnumerable<EntityBasic>>()
                  .ConvertUsing(results => results.Select(Mapper.Map<EntityBasic>).ToList());
        }
    }
}
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

namespace Umbraco.Web.Models.Mapping
{
    internal class EntityModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<UmbracoEntity, EntityBasic>()
                .ForMember(basic => basic.Icon, expression => expression.MapFrom(entity => entity.ContentTypeIcon))
                .ForMember(x => x.Alias, expression => expression.Ignore());

            config.CreateMap<PropertyType, EntityBasic>()
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-box"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<PropertyGroup, EntityBasic>()
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-tab"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                //in v6 the 'alias' is it's lower cased name so we'll stick to that.
                .ForMember(basic => basic.Alias, expression => expression.MapFrom(group => group.Name.ToLowerInvariant()))
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<IUser, EntityBasic>()
                .ForMember(basic => basic.Icon, expression => expression.UseValue("icon-user"))
                .ForMember(basic => basic.Path, expression => expression.UseValue(""))
                .ForMember(basic => basic.ParentId, expression => expression.UseValue(-1))
                .ForMember(basic => basic.Alias, expression => expression.MapFrom(user => user.Username))
                .ForMember(x => x.AdditionalData, expression => expression.Ignore());

            config.CreateMap<SearchResult, EntityBasic>()
                //default to document icon
                  .ForMember(x => x.Icon, expression => expression.UseValue("icon-document"))
                  .ForMember(x => x.Id, expression => expression.MapFrom(result => result.Id))
                  .ForMember(x => x.Name, expression => expression.Ignore())
                  .ForMember(x => x.Key, expression => expression.Ignore())
                  .ForMember(x => x.ParentId, expression => expression.Ignore())
                  .ForMember(x => x.Alias, expression => expression.Ignore())
                  .ForMember(x => x.Path, expression => expression.Ignore())
                  .ForMember(x => x.AdditionalData, expression => expression.Ignore())
                  .AfterMap((result, basic) =>
                      {
                          basic.Name = result.Fields.ContainsKey("nodeName") ? result.Fields["nodeName"] : "[no name]";
                          if (result.Fields.ContainsKey("__NodeKey"))
                          {
                              Guid key;
                              if (Guid.TryParse(result.Fields["__NodeKey"], out key))
                              {
                                  basic.Key = key;
                              }
                          }
                          if (result.Fields.ContainsKey("ParentID"))
                          {
                              int parentId;
                              if (int.TryParse(result.Fields["ParentID"], out parentId))
                              {
                                  basic.ParentId = parentId;
                              }
                              else
                              {
                                  basic.ParentId = -1;
                              }
                          }
                          basic.Path = result.Fields.ContainsKey("__Path") ? result.Fields["__Path"] : "";
                      });

            config.CreateMap<ISearchResults, IEnumerable<EntityBasic>>()
                  .ConvertUsing(results => results.Select(Mapper.Map<EntityBasic>).ToList());
        }
    }
}
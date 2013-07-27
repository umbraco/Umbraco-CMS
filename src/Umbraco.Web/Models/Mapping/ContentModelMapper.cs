using System;
using System.Linq.Expressions;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares how model mappings for content
    /// </summary>
    internal class ContentModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //FROM IContent TO ContentItemDisplay
            config.CreateMap<IContent, ContentItemDisplay>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IContent>>())
                  .ForMember(
                      dto => dto.Updator,
                      expression => expression.ResolveUsing<CreatorResolver>())
                  .ForMember(
                      dto => dto.Icon,
                      expression => expression.MapFrom(content => content.ContentType.Icon))
                  .ForMember(
                      dto => dto.ContentTypeAlias,
                      expression => expression.MapFrom(content => content.ContentType.Alias))
                  .ForMember(
                      dto => dto.PublishDate,
                      expression => expression.MapFrom(content => GetPublishedDate(content, applicationContext)))
                  .ForMember(display => display.Properties, expression => expression.Ignore())
                  .ForMember(display => display.Tabs, expression => expression.ResolveUsing<TabsAndPropertiesResolver>());

            //FROM IContent TO ContentItemBasic<ContentPropertyBasic, IContent>
            config.CreateMap<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IContent>>())
                  .ForMember(
                      dto => dto.Updator,
                      expression => expression.ResolveUsing<CreatorResolver>())
                  .ForMember(
                      dto => dto.Icon,
                      expression => expression.MapFrom(content => content.ContentType.Icon))
                  .ForMember(
                      dto => dto.ContentTypeAlias,
                      expression => expression.MapFrom(content => content.ContentType.Alias));

            //FROM IContent TO ContentItemDto<IContent>
            config.CreateMap<IContent, ContentItemDto<IContent>>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IContent>>());

            
        }

        /// <summary>
        /// Gets the published date value for the IContent object
        /// </summary>
        /// <param name="content"></param>
        /// <param name="applicationContext"></param>
        /// <returns></returns>
        private static DateTime? GetPublishedDate(IContent content, ApplicationContext applicationContext)
        {
            if (content.Published)
            {
                return content.UpdateDate;
            }
            if (content.HasPublishedVersion())
            {
                var published = applicationContext.Services.ContentService.GetPublishedVersion(content.Id);
                return published.UpdateDate;
            }
            return null;
        }

    }
}
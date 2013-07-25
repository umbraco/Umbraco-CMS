using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using System.Linq;

namespace Umbraco.Web.Models.Mapping
{
    internal class NewContentMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //FROM Property TO ContentPropertyBasic
            config.CreateMap<PropertyGroup, Tab<ContentPropertyDisplay>>()
                  .ForMember(tab => tab.Label, expression => expression.MapFrom(@group => @group.Name))
                  .ForMember(tab => tab.IsActive, expression => expression.UseValue(true))
                  .ForMember(tab => tab.Properties, expression => expression.Ignore());

            //FROM Property TO ContentPropertyBasic
            config.CreateMap<Property, ContentPropertyBasic>()
                .ConvertUsing<ContentPropertyBasicConverter<ContentPropertyBasic>>();

            //FROM Property TO ContentPropertyDto
            config.CreateMap<Property, ContentPropertyDto>()
                  .ConvertUsing(new ContentPropertyDtoConverter(applicationContext));

            //FROM Property TO ContentPropertyDisplay
            config.CreateMap<Property, ContentPropertyDisplay>()
                  .ConvertUsing(new ContentPropertyDisplayConverter(applicationContext));

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
                  .ForMember(display => display.Tabs, expression => expression.Ignore())
                  .AfterMap((content, display) => MapTabsAndProperties(content, display));

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

            //FROM IMedia TO ContentItemBasic<ContentPropertyBasic, IMedia>
            config.CreateMap<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IMedia>>())                  
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

            //FROM IMedia TO ContentItemDto<IMedia>
            config.CreateMap<IMedia, ContentItemDto<IMedia>>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IMedia>>());
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

        private static void MapTabsAndProperties(IContentBase content, TabbedContentItem<ContentPropertyDisplay, IContent> display)
        {
            var aggregateTabs = new List<Tab<ContentPropertyDisplay>>();

            //now we need to aggregate the tabs and properties since we might have duplicate tabs (based on aliases) because
            // of how content composition works. 
            foreach (var propertyGroups in content.PropertyGroups.GroupBy(x => x.Name))
            {
                var aggregateProperties = new List<ContentPropertyDisplay>();

                //there will always be one group with a null parent id (the top-most)
                //then we'll iterate over all of the groups and ensure the properties are
                //added in order so that when they render they are rendered with highest leve
                //parent properties first.
                int? currentParentId = null;
                for (var i = 0; i < propertyGroups.Count(); i++)
                {
                    var current = propertyGroups.Single(x => x.ParentId == currentParentId);
                    aggregateProperties.AddRange(
                        Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(
                            content.GetPropertiesForGroup(current)));
                    currentParentId = current.Id;
                }

                //then we'll just use the root group's data to make the composite tab
                var rootGroup = propertyGroups.Single(x => x.ParentId == null);
                aggregateTabs.Add(new Tab<ContentPropertyDisplay>
                    {
                        Id = rootGroup.Id,
                        Alias = rootGroup.Name,
                        Label = rootGroup.Name,
                        Properties = aggregateProperties,
                        IsActive = false
                    });
            }

            //now add the generic properties tab for any properties that don't belong to a tab
            var orphanProperties = content.GetNonGroupedProperties();

            //now add the generic properties tab
            aggregateTabs.Add(new Tab<ContentPropertyDisplay>
            {
                Id = 0,
                Label = "Generic properties",
                Alias = "Generic properties",
                Properties = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(orphanProperties)
            });

            //set the first tab to active
            aggregateTabs.First().IsActive = true;

            display.Tabs = aggregateTabs;
        }

    }

    

    internal class ContentDisplayConverter : TypeConverter<IContent, ContentItemDisplay>
    {
        protected override ContentItemDisplay ConvertCore(IContent source)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Maps the Creator for content
    /// </summary>
    internal class CreatorResolver : ValueResolver<IContent, UserBasic>
    {
        protected override UserBasic ResolveCore(IContent source)
        {
            return Mapper.Map<IProfile, UserBasic>(source.GetWriterProfile());
        }
    }

    /// <summary>
    /// Maps the Owner for IContentBase
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>
    internal class OwnerResolver<TPersisted> : ValueResolver<TPersisted, UserBasic>
        where TPersisted : IContentBase
    {
        protected override UserBasic ResolveCore(TPersisted source)
        {
            return Mapper.Map<IProfile, UserBasic>(source.GetCreatorProfile());
        }
    }

    /// <summary>
    /// Creates a ContentPropertyDto from a Property
    /// </summary>
    internal class ContentPropertyDisplayConverter : ContentPropertyBasicConverter<ContentPropertyDisplay>
    {
        private readonly ApplicationContext _applicationContext;

        public ContentPropertyDisplayConverter(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        protected override ContentPropertyDisplay ConvertCore(Property originalProp)
        {
            var display = base.ConvertCore(originalProp);

            //set the display properties after mapping
            display.Alias = originalProp.Alias;
            display.Description = originalProp.PropertyType.Description;
            display.Label = originalProp.PropertyType.Name;
            display.Config = _applicationContext.Services.DataTypeService.GetPreValuesByDataTypeId(originalProp.PropertyType.DataTypeDefinitionId);
            if (display.PropertyEditor == null)
            {
                //if there is no property editor it means that it is a legacy data type
                // we cannot support editing with that so we'll just render the readonly value view.
                display.View = GlobalSettings.Path.EnsureEndsWith('/') +
                               "views/propertyeditors/umbraco/readonlyvalue/readonlyvalue.html";
            }
            else
            {
                display.View = display.PropertyEditor.ValueEditor.View;
            }

            return display;
        }
    }

    /// <summary>
    /// Creates a ContentPropertyDto from a Property
    /// </summary>
    internal class ContentPropertyDtoConverter : ContentPropertyBasicConverter<ContentPropertyDto>
    {
        private readonly ApplicationContext _applicationContext;

        public ContentPropertyDtoConverter(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        protected override ContentPropertyDto ConvertCore(Property originalProperty)
        {
            var propertyDto = base.ConvertCore(originalProperty);

            propertyDto.IsRequired = originalProperty.PropertyType.Mandatory;
            propertyDto.ValidationRegExp = originalProperty.PropertyType.ValidationRegExp;
            propertyDto.Alias = originalProperty.Alias;
            propertyDto.Description = originalProperty.PropertyType.Description;
            propertyDto.Label = originalProperty.PropertyType.Name;
            propertyDto.DataType = _applicationContext.Services.DataTypeService.GetDataTypeDefinitionById(originalProperty.PropertyType.DataTypeDefinitionId);
            propertyDto.PropertyEditor = PropertyEditorResolver.Current.GetById(originalProperty.PropertyType.DataTypeId);

            return propertyDto;
        }
    }

    /// <summary>
    /// Creates a base generic ContentPropertyBasic from a Property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ContentPropertyBasicConverter<T> : TypeConverter<Property, T>
        where T : ContentPropertyBasic, new()
    {
        protected override T ConvertCore(Property property)
        {
            var editor = PropertyEditorResolver.Current.GetById(property.PropertyType.DataTypeId);
            if (editor == null)
            {
                //TODO: Remove this check as we shouldn't support this at all!
                var legacyEditor = DataTypesResolver.Current.GetById(property.PropertyType.DataTypeId);
                if (legacyEditor == null)
                {
                    throw new NullReferenceException("The property editor with id " + property.PropertyType.DataTypeId + " does not exist");
                }

                var legacyResult = new T
                {
                    Id = property.Id,
                    Value = property.Value == null ? "" : property.Value.ToString(),
                    Alias = property.Alias
                };
                return legacyResult;
            }
            var result = new T
            {
                Id = property.Id,
                Value = editor.ValueEditor.SerializeValue(property.Value),
                Alias = property.Alias
            };

            result.PropertyEditor = editor;

            return result;
        }
    }

}
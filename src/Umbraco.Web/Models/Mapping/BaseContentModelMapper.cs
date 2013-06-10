using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class BaseContentModelMapper
    {
        protected ApplicationContext ApplicationContext { get; private set; }
        protected ProfileModelMapper ProfileMapper { get; private set; }

        public BaseContentModelMapper(ApplicationContext applicationContext, ProfileModelMapper profileMapper)
        {
            ApplicationContext = applicationContext;
            ProfileMapper = profileMapper;
        }

        protected ContentItemDto<TPersisted> ToContentItemDtoBase<TPersisted>(IContentBase content) 
            where TPersisted : IContentBase
        {
            return CreateContent<ContentItemDto<TPersisted>, ContentPropertyDto, TPersisted>(content, null, (propertyDto, originalProperty, propEditor) =>
                {
                    propertyDto.Alias = originalProperty.Alias;
                    propertyDto.Description = originalProperty.PropertyType.Description;
                    propertyDto.Label = originalProperty.PropertyType.Name;
                    propertyDto.DataType = ApplicationContext.Services.DataTypeService.GetDataTypeDefinitionById(originalProperty.PropertyType.DataTypeDefinitionId);
                    propertyDto.PropertyEditor = PropertyEditorResolver.Current.GetById(originalProperty.PropertyType.DataTypeId);
                });
        }

        protected ContentItemBasic<ContentPropertyBasic, TPersisted> ToContentItemSimpleBase<TPersisted>(IContentBase content) 
            where TPersisted : IContentBase
        {
            return CreateContent<ContentItemBasic<ContentPropertyBasic, TPersisted>, ContentPropertyBasic, TPersisted>(content, null, null);
        } 

        protected IList<Tab<ContentPropertyDisplay>> GetTabs(IContentBase content)
        {
            var tabs = content.PropertyGroups.Select(propertyGroup =>
                {
                    //get the properties for the current tab
                    var propertiesForTab = content.GetPropertiesForGroup(propertyGroup).ToArray();

                    //convert the properties to ContentPropertyDisplay objects
                    var displayProperties = propertiesForTab
                        .Select(ToContentPropertyDisplay);

                    //return the tab with the tab properties
                    return new Tab<ContentPropertyDisplay>
                        {
                            Id = propertyGroup.Id,
                            Alias = propertyGroup.Name,
                            Label = propertyGroup.Name,
                            Properties = displayProperties
                        };
                }).ToList();

            //now add the generic properties tab for any properties that don't belong to a tab
            var orphanProperties = content.GetNonGroupedProperties();

            //now add the generic properties tab
            tabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = 0,
                    Label = "Generic properties",
                    Alias = "Generic properties",
                    Properties = orphanProperties.Select(ToContentPropertyDisplay).ToArray()
                });

            return tabs;
        }

        protected TContent CreateContent<TContent, TContentProperty, TPersisted>(IContentBase content,
            Action<TContent, IContentBase> contentCreatedCallback = null,
            Action<TContentProperty, Property, PropertyEditor> propertyCreatedCallback = null,
            bool createProperties = true)
            where TContent : ContentItemBasic<TContentProperty, TPersisted>, new()
            where TContentProperty : ContentPropertyBasic, new() 
            where TPersisted : IContentBase
        {
            var result = new TContent
                {
                    Id = content.Id,
                    Owner = ProfileMapper.ToBasicUser(content.GetCreatorProfile()),
                
                    ParentId = content.ParentId,
                    UpdateDate = content.UpdateDate,
                    CreateDate = content.CreateDate,
                    Name = content.Name
                };
            if (createProperties)
                result.Properties = content.Properties.Select(p => CreateProperty(p, propertyCreatedCallback)).ToArray();
            if (contentCreatedCallback != null)
                contentCreatedCallback(result, content);
            return result;
        }

        protected ContentPropertyDisplay ToContentPropertyDisplay(Property property)
        {
            return CreateProperty<ContentPropertyDisplay>(property, (display, originalProp, propEditor) =>
                {
                    //set the display properties after mapping
                    display.Alias = originalProp.Alias;
                    display.Description = originalProp.PropertyType.Description;
                    display.Label = property.PropertyType.Name;
                    display.Config = ApplicationContext.Services.DataTypeService.GetPreValuesByDataTypeId(property.PropertyType.DataTypeDefinitionId);
                    if (propEditor == null)
                    {
                        //if there is no property editor it means that it is a legacy data type
                        // we cannot support editing with that so we'll just render the readonly value view.
                        display.View = GlobalSettings.Path.EnsureEndsWith('/') +
                                       "views/propertyeditors/umbraco/readonlyvalue/readonlyvalue.html";
                    }
                    else
                    {
                        display.View = propEditor.ValueEditor.View;
                    }
                
                });
        }

        /// <summary>
        /// Creates the property with the basic property values mapped
        /// </summary>
        /// <typeparam name="TContentProperty"></typeparam>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected static TContentProperty CreateProperty<TContentProperty>(
            Property property,
            Action<TContentProperty, Property, PropertyEditor> callback = null)
            where TContentProperty : ContentPropertyBasic, new()
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

                var legacyResult = new TContentProperty
                    {
                        Id = property.Id,
                        Value = property.Value.ToString(),
                        Alias = property.Alias
                    };
                if (callback != null) callback(legacyResult, property, null);
                return legacyResult;
            }
            var result = new TContentProperty
                {
                    Id = property.Id,
                    Value = editor.ValueEditor.SerializeValue(property.Value),
                    Alias = property.Alias
                };
            if (callback != null) callback(result, property, editor);
            return result;
        }
    }
}
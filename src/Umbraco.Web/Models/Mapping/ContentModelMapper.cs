using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentModelMapper
    {
        private readonly ApplicationContext _applicationContext;
        private readonly ProfileModelMapper _profileMapper;

        public ContentModelMapper(ApplicationContext applicationContext, ProfileModelMapper profileMapper)
        {
            _applicationContext = applicationContext;
            _profileMapper = profileMapper;
        }

        private ContentPropertyDisplay ToContentPropertyDisplay(Property property)
        {
            return CreateProperty<ContentPropertyDisplay>(property, (display, originalProp, propEditor) =>
                {
                    //set the display properties after mapping
                    display.Alias = originalProp.Alias;
                    display.Description = originalProp.PropertyType.Description;
                    display.Label = property.PropertyType.Name;
                    display.Config = _applicationContext.Services.DataTypeService.GetPreValuesByDataTypeId(property.PropertyType.DataTypeDefinitionId);
                    display.View = propEditor.ValueEditor.View;
                });
        }

        public ContentItemDisplay ToContentItemDisplay(IContent content)
        {
            //create the list of tabs for properties assigned to tabs.
            var tabs = content.PropertyGroups.Select(propertyGroup =>
                {
                    //get the properties for the current tab
                    var propertiesForTab = content.GetPropertiesForGroup(propertyGroup);

                    //convert the properties to ContentPropertyDisplay objects
                    var displayProperties = propertiesForTab
                        .Select(ToContentPropertyDisplay);

                    //return the tab with the tab properties
                    return new Tab<ContentPropertyDisplay>
                        {
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
                    Label = "Generic properties",
                    Alias = "Generic properties",
                    Properties = orphanProperties.Select(ToContentPropertyDisplay)
                });
            
            var result = CreateContent<ContentItemDisplay, ContentPropertyDisplay>(content, (display, originalContent) =>
                {
                    //set display props after the normal properties are alraedy mapped
                    display.Name = originalContent.Name;
                    display.Tabs = tabs;
                    display.Icon = originalContent.ContentType.Icon;
                    //look up the published version of this item if it is not published
                    if (content.Published)
                    {
                        display.PublishDate = content.UpdateDate;
                    }
                    else if (content.HasPublishedVersion())
                    {
                        var published = _applicationContext.Services.ContentService.GetPublishedVersion(content.Id);
                        display.PublishDate = published.UpdateDate;
                    }
                    else
                    {
                        display.PublishDate = null;
                    }
                    
                }, null, false);

            return result;
        }

        internal ContentItemDto ToContentItemDto(IContent content)
        {
            return CreateContent<ContentItemDto, ContentPropertyDto>(content, null, (propertyDto, originalProperty, propEditor) =>
                {
                    propertyDto.Alias = originalProperty.Alias;
                    propertyDto.Description = originalProperty.PropertyType.Description;
                    propertyDto.Label = originalProperty.PropertyType.Name;
                    propertyDto.DataType = _applicationContext.Services.DataTypeService.GetDataTypeDefinitionById(originalProperty.PropertyType.DataTypeDefinitionId);
                    propertyDto.PropertyEditor = PropertyEditorResolver.Current.GetById(originalProperty.PropertyType.DataTypeId);
                });
        }

        /// <summary>
        /// Creates a new content item
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <typeparam name="TContentProperty"></typeparam>
        /// <param name="content"></param>
        /// <param name="contentCreatedCallback"></param>
        /// <param name="propertyCreatedCallback"></param>
        /// <param name="createProperties"></param>
        /// <returns></returns>
        private TContent CreateContent<TContent, TContentProperty>(IContent content,
            Action<TContent, IContent> contentCreatedCallback = null,
            Action<TContentProperty, Property, PropertyEditor> propertyCreatedCallback = null, 
            bool createProperties = true)
            where TContent : ContentItemBasic<TContentProperty>, new()
            where TContentProperty : ContentPropertyBase, new()
        {
            var result = new TContent
                {
                    Id = content.Id,
                    Owner = _profileMapper.ToBasicUser(content.GetCreatorProfile()),
                    Updator = _profileMapper.ToBasicUser(content.GetWriterProfile()),
                    ParentId = content.ParentId,                                     
                    UpdateDate = content.UpdateDate,
                    CreateDate = content.CreateDate
                };
            if (createProperties) 
                result.Properties = content.Properties.Select(p => CreateProperty(p, propertyCreatedCallback));
            if (contentCreatedCallback != null) 
                contentCreatedCallback(result, content);
            return result;
        }

        /// <summary>
        /// Creates the property with the basic property values mapped
        /// </summary>
        /// <typeparam name="TContentProperty"></typeparam>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private static TContentProperty CreateProperty<TContentProperty>(
            Property property, 
            Action<TContentProperty, Property, PropertyEditor> callback = null)
            where TContentProperty : ContentPropertyBase, new()
        {
            var editor = PropertyEditorResolver.Current.GetById(property.PropertyType.DataTypeId);
            if (editor == null)
            {
                throw new NullReferenceException("The property editor with id " + property.PropertyType.DataTypeId + " does not exist");
            }
            var result = new TContentProperty
                {
                    Id = property.Id,
                    Value = editor.ValueEditor.SerializeValue(property.Value)
                };
            if (callback != null) callback(result, property, editor);
            return result;
        }
    }
}

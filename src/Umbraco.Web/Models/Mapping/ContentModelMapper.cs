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
    internal class BaseContentModelMapper
    {
         protected ApplicationContext ApplicationContext { get; private set; }
         protected ProfileModelMapper ProfileMapper { get; private set; }

        public BaseContentModelMapper(ApplicationContext applicationContext, ProfileModelMapper profileMapper)
        {
            ApplicationContext = applicationContext;
            ProfileMapper = profileMapper;
        }

        internal ContentItemDto ToContentItemDto(IContentBase content)
        {
            return CreateContent<ContentItemDto, ContentPropertyDto>(content, null, (propertyDto, originalProperty, propEditor) =>
            {
                propertyDto.Alias = originalProperty.Alias;
                propertyDto.Description = originalProperty.PropertyType.Description;
                propertyDto.Label = originalProperty.PropertyType.Name;
                propertyDto.DataType = ApplicationContext.Services.DataTypeService.GetDataTypeDefinitionById(originalProperty.PropertyType.DataTypeDefinitionId);
                propertyDto.PropertyEditor = PropertyEditorResolver.Current.GetById(originalProperty.PropertyType.DataTypeId);
            });
        }

        internal ContentItemBasic<ContentPropertyBasic> ToContentItemSimple(IContentBase content)
        {
            return CreateContent<ContentItemBasic<ContentPropertyBasic>, ContentPropertyBasic>(content, null, null);
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

        protected TContent CreateContent<TContent, TContentProperty>(IContentBase content,
            Action<TContent, IContentBase> contentCreatedCallback = null,
            Action<TContentProperty, Property, PropertyEditor> propertyCreatedCallback = null,
            bool createProperties = true)
            where TContent : ContentItemBasic<TContentProperty>, new()
            where TContentProperty : ContentPropertyBasic, new()
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
                if (propEditor != null)
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

    internal class ContentModelMapper : BaseContentModelMapper
    {
       
        public ContentModelMapper(ApplicationContext applicationContext, ProfileModelMapper profileMapper)
            : base(applicationContext, profileMapper)
        {
        }

        public ContentItemDisplay ToContentItemDisplay(IContent content)
        {
            //create the list of tabs for properties assigned to tabs.
            var tabs = GetTabs(content);
            
            var result = CreateContent<ContentItemDisplay, ContentPropertyDisplay>(content, (display, originalContent) =>
                {
                    //fill in the rest
                    display.Updator = ProfileMapper.ToBasicUser(content.GetWriterProfile());
                    display.ContentTypeAlias = content.ContentType.Alias;
                    display.Icon = content.ContentType.Icon;

                    //set display props after the normal properties are alraedy mapped
                    display.Name = originalContent.Name;
                    display.Tabs = tabs;                    
                    //look up the published version of this item if it is not published
                    if (content.Published)
                    {
                        display.PublishDate = content.UpdateDate;
                    }
                    else if (content.HasPublishedVersion())
                    {
                        var published = ApplicationContext.Services.ContentService.GetPublishedVersion(content.Id);
                        display.PublishDate = published.UpdateDate;
                    }
                    else
                    {
                        display.PublishDate = null;
                    }
                    
                }, null, false);

            return result;
        }
        
    }
}

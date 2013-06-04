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
    internal class ContentTypeModelMapper
    {
        private readonly ApplicationContext _applicationContext;

        public ContentTypeModelMapper(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public ContentTypeBasic ToContentItemBasic(IContentType contentType)
        {
            return new ContentTypeBasic
                {
                    Alias = contentType.Alias,
                    Id = contentType.Id,
                    Description = contentType.Description,
                    Icon = contentType.Icon,
                    Name = contentType.Name
                };
        }
    }


    internal class ContentModelMapper
    {
        private readonly ApplicationContext _applicationContext;

        public ContentModelMapper(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        private ContentPropertyDisplay ToContentPropertyDisplay(Property property)
        {
            var editor = PropertyEditorResolver.Current.GetById(property.PropertyType.DataTypeId);
            if (editor == null)
            {
                throw new NullReferenceException("The property editor with id " + property.PropertyType.DataTypeId + " does not exist");
            }
            return new ContentPropertyDisplay
            {
                Alias = property.Alias,
                Id = property.Id,
                Description = property.PropertyType.Description,
                Label = property.PropertyType.Name,
                Config = _applicationContext.Services.DataTypeService.GetPreValuesByDataTypeId(property.PropertyType.DataTypeDefinitionId),
                Value = editor.ValueEditor.SerializeValue(property.Value),
                View = editor.ValueEditor.View
            };
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

            return new ContentItemDisplay
                {
                    Id = content.Id,
                    Name = content.Name,
                    Tabs = tabs
                };
        }

        internal ContentItemDto ToContentItemDto(IContent content)
        {
            return new ContentItemDto
                {
                    Id = content.Id,
                    Properties = content.Properties.Select(p => new ContentPropertyDto
                        {
                            Alias = p.Alias,
                            Description = p.PropertyType.Description,
                            Label = p.PropertyType.Name,
                            Id = p.Id,
                            DataType = _applicationContext.Services.DataTypeService.GetDataTypeDefinitionById(p.PropertyType.DataTypeDefinitionId),
                            PropertyEditor = PropertyEditorResolver.Current.GetById(p.PropertyType.DataTypeId)
                        }).ToList()
                };
        }

    }
}

using AutoMapper;
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
   
    internal class PropertyTypeGroupResolver : ValueResolver<IContentType, IEnumerable<PropertyTypeGroupDisplay>>
    {
        private readonly ApplicationContext _applicationContext;
        private readonly Lazy<PropertyEditorResolver> _propertyEditorResolver;

        public PropertyTypeGroupResolver(ApplicationContext applicationContext, Lazy<PropertyEditorResolver> propertyEditorResolver)
        {
            _applicationContext = applicationContext;
            _propertyEditorResolver = propertyEditorResolver;
        }


        protected override IEnumerable<PropertyTypeGroupDisplay> ResolveCore(IContentType source)
        {
            var groups = new Dictionary<int,PropertyTypeGroupDisplay>();
            
            //pull all tabs from all inherited composite types
            foreach (var ct in source.ContentTypeComposition)
            {
                foreach(var tab in ct.CompositionPropertyGroups){
                    var group = new PropertyTypeGroupDisplay() { Id = tab.Id, Inherited = true, Name = tab.Name, SortOrder = tab.SortOrder };
                    group.ContentTypeId = ct.Id;
                    if (tab.ParentId.HasValue)
                        group.ParentGroupId = tab.ParentId.Value;

                    group.Properties = MapProperties(tab.PropertyTypes, ct.Id);
                    groups.Add(tab.Id, group);
                }
            }

            //pull from own groups
            foreach (var ownTab in source.CompositionPropertyGroups)
            {
                PropertyTypeGroupDisplay group;
                
                //if already added
                if (groups.ContainsKey(ownTab.Id))
                    group = groups[ownTab.Id];

                //if parent
                else if (ownTab.ParentId.HasValue && groups.ContainsKey(ownTab.ParentId.Value))
                    group = groups[ownTab.ParentId.Value];

                else
                {
                    //if own
                    group = new PropertyTypeGroupDisplay() { Id = ownTab.Id, Inherited = false, Name = ownTab.Name, SortOrder = ownTab.SortOrder, ContentTypeId = source.Id };
                    groups.Add(ownTab.Id, group);
                }

                //merge the properties
                var mergedProperties = new List<PropertyTypeDisplay>();
                mergedProperties.AddRange(group.Properties);

                var newproperties = MapProperties( ownTab.PropertyTypes , source.Id).Where(x => mergedProperties.Any( y => y.Id == x.Id ) == false);
                mergedProperties.AddRange(newproperties);

                group.Properties = mergedProperties.OrderBy(x => x.SortOrder);
            }

            var genericProperties = source.CompositionPropertyTypes.Where(x => x.PropertyGroupId == null);
            if (genericProperties.Any())
            {
                var genericTab = new PropertyTypeGroupDisplay() { Id = 0, Name = "Generic properties", ParentGroupId = 0, ContentTypeId = source.Id, SortOrder = 999, Inherited = false };
                genericTab.Properties = MapProperties(genericProperties, source.Id);
                groups.Add(0, genericTab);
            }

            return groups.Values.OrderBy(x => x.SortOrder);
        }

      /*  private IEnumerable<PropertyTypeGroupDisplay> MapChildGroups(PropertyTypeGroupDisplay parent, IEnumerable<PropertyGroup> groups)
        {
            var mappedGroups = new List<PropertyTypeGroupDisplay>();
            
            foreach (var child in groups.Where(x => x.ParentId == parent.Id))
            {
                var mapped = new PropertyTypeGroupDisplay() { Id = child.Id, ParentGroupId = child.ParentId.Value, Name = child.Name, SortOrder = child.SortOrder };
                mapped.Name += child.PropertyTypes.Count.ToString();
                mapped.Properties = MapProperties(child.PropertyTypes);               
                mappedGroups.Add(mapped);
            }

            return mappedGroups;
        }
        */
        private IEnumerable<PropertyTypeDisplay> MapProperties(IEnumerable<PropertyType> properties, int contentTypeId)
        {
            var mappedProperties = new List<PropertyTypeDisplay>();
            foreach (var p in properties)
            {
                var editor = _propertyEditorResolver.Value.GetByAlias(p.PropertyEditorAlias);
                var preVals = _applicationContext.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(p.DataTypeDefinitionId);

                mappedProperties.Add(
                    new PropertyTypeDisplay()
                    {
                        Id = p.Id,
                        Alias = p.Alias,
                        Description = p.Description,
                        Editor = p.PropertyEditorAlias,
                        Validation = new PropertyTypeValidation() { Mandatory = p.Mandatory, Pattern = p.ValidationRegExp },
                        Label = p.Name,
                        View = editor.ValueEditor.View,
                        Config = editor.PreValueEditor.ConvertDbToEditor(editor.DefaultPreValues, preVals) ,
                        Value = "",
                        ContentTypeId = contentTypeId
                    });
            }

            return mappedProperties;
        }
    }
}

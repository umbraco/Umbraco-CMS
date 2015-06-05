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

    internal class PropertyTypeGroupResolver : ValueResolver<IContentTypeComposition, IEnumerable<PropertyTypeGroupDisplay>>
    {
        private readonly ApplicationContext _applicationContext;
        private readonly Lazy<PropertyEditorResolver> _propertyEditorResolver;

        public PropertyTypeGroupResolver(ApplicationContext applicationContext, Lazy<PropertyEditorResolver> propertyEditorResolver)
        {
            _applicationContext = applicationContext;
            _propertyEditorResolver = propertyEditorResolver;
        }


        protected override IEnumerable<PropertyTypeGroupDisplay> ResolveCore(IContentTypeComposition source)
        {
            var groups = new Dictionary<int,PropertyTypeGroupDisplay>();

            //for storing generic properties
            var genericProperties = new List<PropertyTypeDisplay>();

            
            //iterate through all composite types
            foreach (var ct in source.ContentTypeComposition)
            {
                //process each tab
                foreach(var tab in ct.CompositionPropertyGroups){
                    var group = new PropertyTypeGroupDisplay() { Id = tab.Id, Inherited = true, Name = tab.Name, SortOrder = tab.SortOrder };
                    group.ContentTypeId = ct.Id;
                    group.ParentTabContentTypes = new[] { ct.Id };
                    group.ParentTabContentTypeNames = new[] { ct.Name };

                    if (tab.ParentId.HasValue)
                        group.ParentGroupId = tab.ParentId.Value;

                    group.Properties = MapProperties(tab.PropertyTypes, ct, tab.Id, true);
                    groups.Add(tab.Id, group);
                }

                //process inherited generic properties
                var inheritedGenProperties = ct.CompositionPropertyTypes.Where(x => x.PropertyGroupId == null);
                if (inheritedGenProperties.Any())
                    genericProperties.AddRange(MapProperties(inheritedGenProperties, ct, 0, true));
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

                var newproperties = MapProperties( ownTab.PropertyTypes , source, ownTab.Id, false).Where(x => mergedProperties.Any( y => y.Id == x.Id ) == false);
                mergedProperties.AddRange(newproperties);

                group.Properties = mergedProperties.OrderBy(x => x.SortOrder);
            }


            //get all generic properties not already mapped to the generic props collection 
            var ownGenericProperties = source.CompositionPropertyTypes.Where(x => x.PropertyGroupId == null && genericProperties.Any(y => y.Id == x.Id));
            genericProperties.AddRange(MapProperties(ownGenericProperties, source, 0, false));

            if (genericProperties.Any())
            {
                var genericTab = new PropertyTypeGroupDisplay() { Id = 0, Name = "Generic properties", ParentGroupId = 0, ContentTypeId = source.Id, SortOrder = 999, Inherited = false };
                groups.Add(0, genericTab);
            }


            //merge tabs based on names (magic and insanity)
            var nameGroupedGroups = groups.Values.GroupBy(x => x.Name);
            if (nameGroupedGroups.Any(x => x.Count() > 1))
            {
                var sortedGroups = new List<PropertyTypeGroupDisplay>();

                foreach (var groupOfGroups in nameGroupedGroups)
                {
                    //single name groups
                    if(groupOfGroups.Count() == 1)
                        sortedGroups.Add(groupOfGroups.First());
                    else{
                        //multiple name groups

                        //find the mother tab - if we have our own use it. otherwise pick a random inherited one - since it wont matter
                        var mainTab = groupOfGroups.FirstOrDefault(x => x.Inherited == false);
                        if (mainTab == null)
                            mainTab = groupOfGroups.First();


                        //take all properties from all the other tabs and merge into one tab
                        var properties = new List<PropertyTypeDisplay>();
                        properties.AddRange(mainTab.Properties);
                        properties.AddRange(groupOfGroups.Where(x => x.Id != mainTab.Id).SelectMany(x => x.Properties));
                        mainTab.Properties = properties;

                        //lock the tab
                        mainTab.Inherited = true;

                        //collect all the involved content types
                        mainTab.ParentTabContentTypes = groupOfGroups.Where(x => x.ContentTypeId != source.Id).Select(x => x.ContentTypeId);
                        sortedGroups.Add(mainTab);
                    }
                }

                return sortedGroups.OrderBy(x => x.SortOrder);
            }


            return groups.Values.OrderBy(x => x.SortOrder);
        }

        private IEnumerable<PropertyTypeDisplay> MapProperties(IEnumerable<PropertyType> properties, IContentTypeBase contentType, int groupId, bool inherited)
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
                        ContentTypeId = contentType.Id,
                        ContentTypeName = contentType.Name,
                        GroupId = groupId,
                        Inherited = inherited
                    });
            }

            return mappedProperties;
        }
    }
}

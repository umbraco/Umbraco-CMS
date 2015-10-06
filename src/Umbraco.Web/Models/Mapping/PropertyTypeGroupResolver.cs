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
    

    internal class PropertyTypeGroupResolver : ValueResolver<IContentTypeComposition, IEnumerable<PropertyGroupDisplay>>
    {
        private readonly ApplicationContext _applicationContext;
        private readonly Lazy<PropertyEditorResolver> _propertyEditorResolver;

        public PropertyTypeGroupResolver(ApplicationContext applicationContext, Lazy<PropertyEditorResolver> propertyEditorResolver)
        {
            _applicationContext = applicationContext;
            _propertyEditorResolver = propertyEditorResolver;
        }

        /// <summary>
        /// Will recursively check all compositions (of compositions) to find the content type that contains the 
        /// tabId being searched for.
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="tabId"></param>
        /// <returns></returns>
        private IContentTypeComposition GetContentTypeFromTabId(IContentTypeComposition ct, int tabId)
        {
            if (ct.PropertyGroups.Any(x => x.Id == tabId)) return ct;

            foreach (var composition in ct.ContentTypeComposition)
            {
                var found = GetContentTypeFromTabId(composition, tabId);
                if (found != null) return found;
            }

            return null;
        }

        protected override IEnumerable<PropertyGroupDisplay> ResolveCore(IContentTypeComposition source)
        {
            var groups = new Dictionary<int,PropertyGroupDisplay>();

            //for storing generic properties
            var genericProperties = new List<PropertyTypeDisplay>();

            //add groups directly assigned to this content type
            foreach (var tab in source.PropertyGroups)
            {
                var group = new PropertyGroupDisplay()
                {
                    Id = tab.Id, Inherited = false, Name = tab.Name, SortOrder = tab.SortOrder, ContentTypeId = source.Id
                };

                if (tab.ParentId.HasValue)
                    group.ParentGroupId = tab.ParentId.Value;

                group.Properties = MapProperties(tab.PropertyTypes, source, tab.Id, false);
                groups.Add(tab.Id, group);
            }         

            //add groups not assigned to this content type (via compositions)
            foreach (var tab in source.CompositionPropertyGroups)
            {
                if (groups.ContainsKey(tab.Id)) continue;

                var composition = GetContentTypeFromTabId(source, tab.Id);
                if (composition == null) 
                    throw new InvalidOperationException("The tabId " + tab.Id + " was not found on any of the content type's compositions");

                var group = new PropertyGroupDisplay()
                {
                    Id = tab.Id, Inherited = true, Name = tab.Name, SortOrder = tab.SortOrder, ContentTypeId = composition.Id,
                    ParentTabContentTypes = new[] {composition.Id},
                    ParentTabContentTypeNames = new[] {composition.Name}
                };

                if (tab.ParentId.HasValue)
                    group.ParentGroupId = tab.ParentId.Value;

                group.Properties = MapProperties(tab.PropertyTypes, composition, tab.Id, true);
                groups.Add(tab.Id, group);
            }

            //process generic properties assigned to this content item (without a group)

            //NOTE: -666 is just a thing that is checked during mapping the other direction, it's a 'special' id 

            var entityGenericProperties = source.PropertyTypes.Where(x => x.PropertyGroupId == null);
            genericProperties.AddRange(MapProperties(entityGenericProperties, source, -666, false));

            //process generic properties from compositions (ensures properties are flagged as inherited)
            var currentGenericPropertyIds = genericProperties.Select(x => x.Id).ToArray();
            var compositionGenericProperties = source.CompositionPropertyTypes
                .Where(x => x.PropertyGroupId == null && currentGenericPropertyIds.Contains(x.Id) == false);
            genericProperties.AddRange(MapProperties(compositionGenericProperties, source, -666, true));

            //now add the group if there are any generic props
            if (genericProperties.Any())
            {
                var genericTab = new PropertyGroupDisplay
                {
                    Id = -666, Name = "Generic properties", ParentGroupId = 0, ContentTypeId = source.Id, SortOrder = 999, Inherited = false, Properties = genericProperties
                };
                groups.Add(0, genericTab);
            }


            //merge tabs based on names (magic and insanity)
            var nameGroupedGroups = groups.Values.GroupBy(x => x.Name).ToArray();
            if (nameGroupedGroups.Any(x => x.Count() > 1))
            {
                var sortedGroups = new List<PropertyGroupDisplay>();

                foreach (var groupOfGroups in nameGroupedGroups)
                {
                    //single name groups
                    if (groupOfGroups.Count() == 1)
                    {
                        sortedGroups.Add(groupOfGroups.First());
                    }
                    else
                    {
                        //multiple name groups

                        //find the mother tab - if we have our own use it. otherwise pick a random inherited one - since it wont matter
                        var mainTab = groupOfGroups.FirstOrDefault(x => x.Inherited == false) ?? groupOfGroups.First();

                        //take all properties from all the other tabs and merge into one tab
                        var properties = new List<PropertyTypeDisplay>();
                        properties.AddRange(groupOfGroups.Where(x => x.Id != mainTab.Id).SelectMany(x => x.Properties));
                        properties.AddRange(mainTab.Properties);
                        mainTab.Properties = properties;

                        //lock the tab
                        mainTab.Inherited = true;

                        //collect all the involved content types
                        var parents = groupOfGroups.Where(x => x.ContentTypeId != source.Id).ToList();
                        mainTab.ParentTabContentTypes = parents.SelectMany(x => x.ParentTabContentTypes).ToArray();
                        mainTab.ParentTabContentTypeNames = parents.SelectMany(x => x.ParentTabContentTypeNames).ToArray();
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
            foreach (var p in properties.Where(x => x.DataTypeDefinitionId != 0) )
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
                        //Value = "",
                        ContentTypeId = contentType.Id,
                        ContentTypeName = contentType.Name,
                        GroupId = groupId,
                        Inherited = inherited,
                        DataTypeId = p.DataTypeDefinitionId,
                        SortOrder = p.SortOrder
                    });
            }

            return mappedProperties;
        }
    }
}

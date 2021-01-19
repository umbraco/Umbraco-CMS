﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class PropertyTypeGroupMapper<TPropertyType>
        where TPropertyType : PropertyTypeDisplay, new()
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILogger _logger;

        public PropertyTypeGroupMapper(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, ILogger logger)
        {
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the content type that defines a property group, within a composition.
        /// </summary>
        /// <param name="contentType">The composition.</param>
        /// <param name="propertyGroupId">The identifier of the property group.</param>
        /// <returns>The composition content type that defines the specified property group.</returns>
        private static IContentTypeComposition GetContentTypeForPropertyGroup(IContentTypeComposition contentType, int propertyGroupId)
        {
            // test local groups
            if (contentType.PropertyGroups.Any(x => x.Id == propertyGroupId))
                return contentType;

            // test composition types groups
            // .ContentTypeComposition is just the local ones, not recursive,
            // so we have to recurse here
            return contentType.ContentTypeComposition
                .Select(x => GetContentTypeForPropertyGroup(x, propertyGroupId))
                .FirstOrDefault(x => x != null);
        }

        /// <summary>
        /// Gets the content type that defines a property group, within a composition.
        /// </summary>
        /// <param name="contentType">The composition.</param>
        /// <param name="propertyTypeId">The identifier of the property type.</param>
        /// <returns>The composition content type that defines the specified property group.</returns>
        private static IContentTypeComposition GetContentTypeForPropertyType(IContentTypeComposition contentType, int propertyTypeId)
        {
            // test local property types
            if (contentType.PropertyTypes.Any(x => x.Id == propertyTypeId))
                return contentType;

            // test composition property types
            // .ContentTypeComposition is just the local ones, not recursive,
            // so we have to recurse here
            return contentType.ContentTypeComposition
                .Select(x => GetContentTypeForPropertyType(x, propertyTypeId))
                .FirstOrDefault(x => x != null);
        }

        public IEnumerable<PropertyGroupDisplay<TPropertyType>> Map(IContentTypeComposition source)
        {
            // deal with groups
            var groups = new List<PropertyGroupDisplay<TPropertyType>>();

            // add groups local to this content type
            foreach (var tab in source.PropertyGroups)
            {
                var group = new PropertyGroupDisplay<TPropertyType>
                {
                    Id = tab.Id,
                    Inherited = false,
                    Name = tab.Name,
                    SortOrder = tab.SortOrder,
                    ContentTypeId = source.Id
                };

                group.Properties = MapProperties(tab.PropertyTypes, source, tab.Id, false);
                groups.Add(group);
            }

            // add groups inherited through composition
            var localGroupIds = groups.Select(x => x.Id).ToArray();
            foreach (var tab in source.CompositionPropertyGroups)
            {
                // skip those that are local to this content type
                if (localGroupIds.Contains(tab.Id)) continue;

                // get the content type that defines this group
                var definingContentType = GetContentTypeForPropertyGroup(source, tab.Id);
                if (definingContentType == null)
                    throw new Exception("PropertyGroup with id=" + tab.Id + " was not found on any of the content type's compositions.");

                var group = new PropertyGroupDisplay<TPropertyType>
                {
                    Id = tab.Id,
                    Inherited = true,
                    Name = tab.Name,
                    SortOrder = tab.SortOrder,
                    ContentTypeId = definingContentType.Id,
                    ParentTabContentTypes = new[] { definingContentType.Id },
                    ParentTabContentTypeNames = new[] { definingContentType.Name }
                };

                group.Properties = MapProperties(tab.PropertyTypes, definingContentType, tab.Id, true);
                groups.Add(group);
            }

            // deal with generic properties
            var genericProperties = new List<TPropertyType>();

            // add generic properties local to this content type
            var entityGenericProperties = source.PropertyTypes.Where(x => x.PropertyGroupId == null);
            genericProperties.AddRange(MapProperties(entityGenericProperties, source, PropertyGroupBasic.GenericPropertiesGroupId, false));

            // add generic properties inherited through compositions
            var localGenericPropertyIds = genericProperties.Select(x => x.Id).ToArray();
            var compositionGenericProperties = source.CompositionPropertyTypes
                .Where(x => x.PropertyGroupId == null // generic
                    && localGenericPropertyIds.Contains(x.Id) == false); // skip those that are local
            foreach (var compositionGenericProperty in compositionGenericProperties)
            {
                var definingContentType = GetContentTypeForPropertyType(source, compositionGenericProperty.Id);
                if (definingContentType == null)
                    throw new Exception("PropertyType with id=" + compositionGenericProperty.Id + " was not found on any of the content type's compositions.");
                genericProperties.AddRange(MapProperties(new [] { compositionGenericProperty }, definingContentType, PropertyGroupBasic.GenericPropertiesGroupId, true));
            }

            // if there are any generic properties, add the corresponding tab
            if (genericProperties.Any())
            {
                var genericTab = new PropertyGroupDisplay<TPropertyType>
                {
                    Id = PropertyGroupBasic.GenericPropertiesGroupId,
                    Name = "Generic properties",
                    ContentTypeId = source.Id,
                    SortOrder = 999,
                    Inherited = false,
                    Properties = genericProperties
                };
                groups.Add(genericTab);
            }

            // handle locked properties
            var lockedPropertyAliases = new List<string>();
            // add built-in member property aliases to list of aliases to be locked
            foreach (var propertyAlias in Constants.Conventions.Member.GetStandardPropertyTypeStubs().Keys)
            {
                lockedPropertyAliases.Add(propertyAlias);
            }
            // lock properties by aliases
            foreach (var property in groups.SelectMany(x => x.Properties))
            {
                property.Locked = lockedPropertyAliases.Contains(property.Alias);
            }

            // now merge tabs based on names
            // as for one name, we might have one local tab, plus some inherited tabs
            var groupsGroupsByName = groups.GroupBy(x => x.Name).ToArray();
            groups = new List<PropertyGroupDisplay<TPropertyType>>(); // start with a fresh list
            foreach (var groupsByName in groupsGroupsByName)
            {
                // single group, just use it
                if (groupsByName.Count() == 1)
                {
                    groups.Add(groupsByName.First());
                    continue;
                }

                // multiple groups, merge
                var group = groupsByName.FirstOrDefault(x => x.Inherited == false) // try local
                    ?? groupsByName.First(); // else pick one randomly
                groups.Add(group);

                // in case we use the local one, flag as inherited
                group.Inherited = true;

                // merge (and sort) properties
                var properties = groupsByName.SelectMany(x => x.Properties).OrderBy(x => x.SortOrder).ToArray();
                group.Properties = properties;

                // collect parent group info
                var parentGroups = groupsByName.Where(x => x.ContentTypeId != source.Id).ToArray();
                group.ParentTabContentTypes = parentGroups.SelectMany(x => x.ParentTabContentTypes).ToArray();
                group.ParentTabContentTypeNames = parentGroups.SelectMany(x => x.ParentTabContentTypeNames).ToArray();
            }

            return groups.OrderBy(x => x.SortOrder);
        }

        private IEnumerable<TPropertyType> MapProperties(IEnumerable<PropertyType> properties, IContentTypeBase contentType, int groupId, bool inherited)
        {
            var mappedProperties = new List<TPropertyType>();

            foreach (var p in properties.Where(x => x.DataTypeId != 0).OrderBy(x => x.SortOrder))
            {
                var propertyEditorAlias = p.PropertyEditorAlias;
                var propertyEditor = _propertyEditors[propertyEditorAlias];
                var dataType = _dataTypeService.GetDataType(p.DataTypeId);

                //fixme: Don't explode if we can't find this, log an error and change this to a label
                if (propertyEditor == null)
                {
                    _logger.Error(GetType(),
                        "No property editor could be resolved with the alias: {PropertyEditorAlias}, defaulting to label", p.PropertyEditorAlias);
                    propertyEditorAlias = Constants.PropertyEditors.Aliases.Label;
                    propertyEditor = _propertyEditors[propertyEditorAlias];
                }

                var config = propertyEditor == null
                    ? new Dictionary<string, object>()
                    : dataType.Editor.GetConfigurationEditor().ToConfigurationEditor(dataType.Configuration);

                mappedProperties.Add(new TPropertyType
                {
                    Id = p.Id,
                    Alias = p.Alias,
                    Description = p.Description,
                    LabelOnTop = p.LabelOnTop,
                    Editor = p.PropertyEditorAlias,
                    Validation = new PropertyTypeValidation
                        {
                            Mandatory = p.Mandatory,
                            MandatoryMessage = p.MandatoryMessage,
                            Pattern = p.ValidationRegExp,
                            PatternMessage = p.ValidationRegExpMessage,
                        },
                    Label = p.Name,
                    View = propertyEditor.GetValueEditor().View,
                    Config = config,
                    //Value = "",
                    GroupId = groupId,
                    Inherited = inherited,
                    DataTypeId = p.DataTypeId,
                    DataTypeKey = p.DataTypeKey,
                    DataTypeName = dataType.Name,
                    DataTypeIcon = propertyEditor.Icon,
                    SortOrder = p.SortOrder,
                    ContentTypeId = contentType.Id,
                    ContentTypeName = contentType.Name,
                    AllowCultureVariant = p.VariesByCulture(),
                    AllowSegmentVariant = p.VariesBySegment()
                });
            }

            return mappedProperties;
        }
    }
}

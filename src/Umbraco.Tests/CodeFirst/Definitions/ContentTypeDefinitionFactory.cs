using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.CodeFirst.Attributes;
using umbraco.interfaces;

namespace Umbraco.Tests.CodeFirst.Definitions
{
    public static class ContentTypeDefinitionFactory
    {
        private static ConcurrentDictionary<string, DependencyField> _contentTypeCache = new ConcurrentDictionary<string, DependencyField>();

        public static Lazy<IContentType> GetContentTypeDefinition(Type modelType)
        {
            //Check for BaseType different from ContentTypeBase
            bool hasParent = modelType.BaseType != null && modelType.BaseType != typeof(ContentTypeBase) && modelType.BaseType != typeof(object);
            var parent = new Lazy<IContentType>();
            if(hasParent)
            {
                var isResolved = _contentTypeCache.ContainsKey(modelType.BaseType.FullName);
                parent = isResolved
                             ? _contentTypeCache[modelType.BaseType.FullName].ContentType
                             : GetContentTypeDefinition(modelType.BaseType);
            }

            var contentTypeAttribute = modelType.FirstAttribute<ContentTypeAttribute>();
            var contentTypeAlias = contentTypeAttribute == null ? modelType.Name.ToUmbracoAlias() : contentTypeAttribute.Alias;
            //Check if ContentType already exists by looking it up by Alias.
            var existing = ServiceFactory.ContentTypeService.GetContentType(contentTypeAlias);

            Lazy<IContentType> contentType = contentTypeAttribute == null
                                                 ? PlainPocoConvention(modelType, existing)
                                                 : ContentTypeConvention(contentTypeAttribute, modelType, existing);

            var definitions = new List<PropertyDefinition>();
            int order = 0;
            var objProperties = modelType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
            foreach (var propertyInfo in objProperties)
            {
                var definition = new PropertyDefinition();

                var aliasAttribute = propertyInfo.FirstAttribute<AliasAttribute>();
                definition.Alias = PropertyTypeAliasConvention(aliasAttribute, propertyInfo.Name);
                definition.Name = PropertyTypeNameConvention(aliasAttribute, propertyInfo.Name);

                var descriptionAttribute = propertyInfo.FirstAttribute<DescriptionAttribute>();
                definition.Description = descriptionAttribute != null ? descriptionAttribute.Description : string.Empty;

                var sortOrderAttribute = propertyInfo.FirstAttribute<SortOrderAttribute>();
                definition.Order = sortOrderAttribute != null ? sortOrderAttribute.Order : order;

                var propertyTypeAttribute = propertyInfo.FirstAttribute<PropertyTypeAttribute>();
                definition.Mandatory = propertyTypeAttribute != null && propertyTypeAttribute.Mandatory;
                definition.ValidationRegExp = propertyTypeAttribute == null ? string.Empty : propertyTypeAttribute.ValidationRegExp;
                definition.PropertyGroup = propertyTypeAttribute == null ? "Generic Properties" : propertyTypeAttribute.PropertyGroup;
                definition.DataTypeDefinition = DataTypeConvention(propertyTypeAttribute, propertyInfo.PropertyType);

                //RichtextAttribute convention
                var richtextAttribute = propertyInfo.FirstAttribute<RichtextAttribute>();
                if(richtextAttribute != null)
                {
                    definition.DataTypeDefinition = DataTypeConvention(richtextAttribute, propertyInfo.PropertyType);
                }

                definitions.Add(definition);
                order++;
            }

            //Loop through definitions for PropertyGroups and create those that not already exists
            var groupDefinitions = definitions.DistinctBy(d => d.PropertyGroup);
            foreach (var groupDefinition in groupDefinitions)
            {
                var groupExists = contentType.Value.PropertyGroups.Contains(groupDefinition.PropertyGroup);
                if(groupExists == false)
                {
                    var propertyGroup = new PropertyGroup {Name = groupDefinition.PropertyGroup};
                    contentType.Value.PropertyGroups.Add(propertyGroup);
                }
            }

            //Loop through definitions for PropertyTypes and add them to the correct PropertyGroup
            foreach (var definition in definitions)
            {
                var group = contentType.Value.PropertyGroups.First(x => x.Name == definition.PropertyGroup);
                //Check if a PropertyType with the same alias already exists, as we don't want to override existing ones
                if(group.PropertyTypes.Contains(definition.Alias)) continue;

                var propertyType = new PropertyType(definition.DataTypeDefinition)
                                       {
                                           Mandatory = definition.Mandatory,
                                           ValidationRegExp = definition.ValidationRegExp,
                                           SortOrder = definition.Order,
                                           Alias = definition.Alias,
                                           Name = definition.Name
                                       };

                group.PropertyTypes.Add(propertyType);
            }

            //If current ContentType has a Parent the ParentId should be set and the ContentType added to the composition.
            if(hasParent)
            {
                contentType.Value.SetLazyParentId(new Lazy<int>(() => parent.Value.Id));
                contentType.Value.AddContentType(parent.Value);
            }
            //Add the resolved ContentType to the internal cache
            var field = new DependencyField {ContentType = contentType, Alias = contentType.Value.Alias};
            var dependencies = new List<string>();
            if(hasParent)
                dependencies.Add(parent.Value.Alias);
            if(contentType.Value.AllowedContentTypes.Any())
            {
                dependencies.AddRange(contentType.Value.AllowedContentTypes.Select(allowed => allowed.Alias));
            }
            field.DependsOn = dependencies.ToArray();
            _contentTypeCache.AddOrUpdate(modelType.FullName, field, (x, y) => field);
            return contentType;
        }

        private static int[] GetTopologicalSortOrder(IList<DependencyField> fields)
        {
            var g = new TopologicalSorter(fields.Count());
            var _indexes = new Dictionary<string, int>();

            //add vertices
            for (int i = 0; i < fields.Count(); i++)
            {
                _indexes[fields[i].Alias.ToLower()] = g.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].DependsOn != null)
                {
                    for (int j = 0; j < fields[i].DependsOn.Length; j++)
                    {
                        g.AddEdge(i,
                            _indexes[fields[i].DependsOn[j].ToLower()]);
                    }
                }
            }

            int[] result = g.Sort();
            return result;

        }

        public static IEnumerable<Lazy<IContentType>> RetrieveMappedContentTypes()
        {
            var fields = _contentTypeCache.Select(x => x.Value).ToList();
            int[] sortOrder = GetTopologicalSortOrder(fields);
            var list = new List<Lazy<IContentType>>();
            for (int i = 0; i < sortOrder.Length; i++)
            {
                var field = fields[sortOrder[i]];
                list.Add(field.ContentType);
                Console.WriteLine(field.Alias);
                if (field.DependsOn != null)
                    foreach (var item in field.DependsOn)
                    {
                        Console.WriteLine(" -{0}", item);
                    }
            }
            list.Reverse();
            return list;
        }

        public static void ClearContentTypeCache()
        {
            _contentTypeCache.Clear();
        }

        /// <summary>
        /// Convention to get a DataTypeDefinition from the PropertyTypeAttribute or the type of the property itself
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IDataTypeDefinition DataTypeConvention(PropertyTypeAttribute attribute, Type type)
        {
            if(attribute != null)
            {
                var instance = Activator.CreateInstance(attribute.Type);
                var dataType = instance as IDataType;
                return GetDataTypeByControlId(dataType.Id);
            }

            return TypeToPredefinedDataTypeConvention(type);
        }

        /// <summary>
        /// Convention to get predefined DataTypeDefinitions based on the Type of the property
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IDataTypeDefinition TypeToPredefinedDataTypeConvention(Type type)
        {
            if(type == typeof(bool))
            {
                return GetDataTypeByControlId(new Guid("38b352c1-e9f8-4fd8-9324-9a2eab06d97a"));// Yes/No DataType
            }
            if(type == typeof(int))
            {
                return GetDataTypeByControlId(new Guid("1413afcb-d19a-4173-8e9a-68288d2a73b8"));// Number DataType
            }
            if(type == typeof(DateTime))
            {
                return GetDataTypeByControlId(new Guid("23e93522-3200-44e2-9f29-e61a6fcbb79a"));// Date Picker DataType
            }

            return GetDataTypeByControlId(new Guid("ec15c1e5-9d90-422a-aa52-4f7622c63bea"));// Standard textfield
        }

        /// <summary>
        /// Gets the <see cref="IDataTypeDefinition"/> from the DataTypeService by its control Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static IDataTypeDefinition GetDataTypeByControlId(Guid id)
        {
            //TODO Create Definition if none exists
            var definition = ServiceFactory.DataTypeService.GetDataTypeDefinitionByControlId(id);
            return definition.FirstOrDefault();
        }

        /// <summary>
        /// Convention to get the Alias of the PropertyType from the AliasAttribute or the property itself
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string PropertyTypeAliasConvention(AliasAttribute attribute, string propertyName)
        {
            return attribute == null ? propertyName.ToUmbracoAlias() : attribute.Alias;
        }

        /// <summary>
        /// Convention to get the Name of the PropertyType from the AliasAttribute or the property itself
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string PropertyTypeNameConvention(AliasAttribute attribute, string propertyName)
        {
            if (attribute == null)
                return propertyName.SplitPascalCasing();

            return string.IsNullOrEmpty(attribute.Name) ? propertyName.SplitPascalCasing() : attribute.Name;
        }

        /// <summary>
        /// Convention that converts a class decorated with the ContentTypeAttribute to an initial ContentType
        /// </summary>
        /// <param name="attribute"><see cref="ContentTypeAttribute"/> to use for mapping a <see cref="IContentType"/></param>
        /// <param name="modelType">Type of the current class</param>
        /// <param name="existing"> </param>
        /// <returns>A Lazy <see cref="IContentType"/></returns>
        private static Lazy<IContentType> ContentTypeConvention(ContentTypeAttribute attribute, Type modelType, IContentType existing)
        {
            var children = attribute.AllowedChildContentTypes == null
                               ? new List<ContentTypeSort>()
                               : AllowedChildContentTypesConvention(
                                   attribute.AllowedChildContentTypes, modelType);
            
            var templates = attribute.AllowedTemplates == null
                                ? new List<ITemplate>()
                                : AllowedTemplatesConvention(attribute.AllowedTemplates);

            if(existing != null)
            {
                if (children.Any())
                    existing.AllowedContentTypes = children;
                
                if (templates.Any())
                    existing.AllowedTemplates = templates;

                return new Lazy<IContentType>(() => existing);
            }

            var contentType = new ContentType(-1)
                                  {
                                      Alias = attribute.Alias,
                                      Description = attribute.Description,
                                      Icon = attribute.IconUrl,
                                      Thumbnail = attribute.Thumbnail,
                                      Name = string.IsNullOrEmpty(attribute.Name)
                                                 ? modelType.Name.SplitPascalCasing()
                                                 : attribute.Name,
                                      AllowedContentTypes = children,
                                      AllowedTemplates = templates
                                  };

            return new Lazy<IContentType>(() => contentType);
        }

        /// <summary>
        /// Convention to resolve referenced templates
        /// </summary>
        /// <param name="templateNames"></param>
        /// <returns></returns>
        private static IEnumerable<ITemplate> AllowedTemplatesConvention(IEnumerable<string> templateNames)
        {
            var templates = new List<ITemplate>();
            var engine = UmbracoSettings.DefaultRenderingEngine;
            foreach (var templateName in templateNames)
            {
                var @alias = engine == RenderingEngine.Mvc
                               ? templateName.Replace(".cshtml", "").Replace(".vbhtml", "")
                               : templateName.Replace(".masterpage", "");
                var name = engine == RenderingEngine.Mvc
                               ? string.Concat(@alias, ".cshtml")
                               : string.Concat(@alias, ".masterpage");

                var template = ServiceFactory.FileService.GetTemplateByAlias(@alias);
                if(template == null)
                {
                    template = new Template(string.Empty, name, @alias) { CreatorId = 0, Content = string.Empty};
                    ServiceFactory.FileService.SaveTemplate(template);
                }
                templates.Add(template);
            }
            return templates;
        }

        /// <summary>
        /// Convention to resolve referenced child content types
        /// </summary>
        /// <param name="types"></param>
        /// <param name="currentType"> </param>
        /// <returns></returns>
        private static IEnumerable<ContentTypeSort> AllowedChildContentTypesConvention(IEnumerable<Type> types, Type currentType)
        {
            var contentTypeSorts = new List<ContentTypeSort>();
            int order = 0;
            foreach (var type in types)
            {
                if(type == currentType) continue;//If the referenced type is equal to the current type we skip it to avoid a circular dependency

                var contentTypeSort = new ContentTypeSort();
                var isResolved = _contentTypeCache.ContainsKey(type.FullName);
                var lazy = isResolved ? _contentTypeCache[type.FullName].ContentType : GetContentTypeDefinition(type);
                contentTypeSort.Id = new Lazy<int>(() => lazy.Value.Id);
                contentTypeSort.Alias = lazy.Value.Alias;
                contentTypeSort.SortOrder = order;
                contentTypeSorts.Add(contentTypeSort);
                order++;
            }
            return contentTypeSorts;
        }

        /// <summary>
        /// Convention that converts a simple POCO to an initial ContentType
        /// </summary>
        /// <param name="modelType">Type of the object to map to a <see cref="IContentType"/></param>
        /// <param name="existing"> </param>
        /// <returns>A Lazy <see cref="IContentType"/></returns>
        private static Lazy<IContentType> PlainPocoConvention(Type modelType, IContentType existing)
        {
            if(existing != null)
                return new Lazy<IContentType>(() => existing);

            var contentType = new ContentType(-1)
                                  {
                                      Alias = modelType.Name.ToUmbracoAlias(),
                                      Description = string.Empty,
                                      Icon = "folder.gif",
                                      Thumbnail = "folder.png",
                                      Name = modelType.Name.SplitPascalCasing(),
                                      AllowedTemplates = new List<ITemplate>(),
                                      AllowedContentTypes = new List<ContentTypeSort>()
                                  };

            return new Lazy<IContentType>(() => contentType);
        }
    }
}
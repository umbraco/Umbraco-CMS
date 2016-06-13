using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.PropertyEditors
{
    internal class NestedContentHelper
    {
        public static PreValueCollection GetPreValuesCollectionByDataTypeId(int dtdId)
        {
            var preValueCollection = (PreValueCollection)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                string.Concat("Umbraco.PropertyEditors.NestedContent.GetPreValuesCollectionByDataTypeId_", dtdId),
                () => ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dtdId));

            return preValueCollection;
        }

        public static string GetContentTypeAliasFromJson(JObject item)
        {
            var contentTypeAliasProperty = item[NestedContentPropertyEditor.ContentTypeAliasPropertyKey];
            if (contentTypeAliasProperty == null)
            {
                return null;
            }

            return contentTypeAliasProperty.ToObject<string>();
        }

        public static IContentType GetContentTypeFromJson(JObject item)
        {
            var contentTypeAlias = GetContentTypeAliasFromJson(item);
            if (string.IsNullOrEmpty(contentTypeAlias))
            {
                return null;
            }

            return ApplicationContext.Current.Services.ContentTypeService.GetContentType(contentTypeAlias);
        }

        public static bool IsNestedContentProperty(PublishedPropertyType publishedProperty)
        {
            return publishedProperty.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.NestedContentAlias);
        }

        public static bool IsSingleNestedContentProperty(PublishedPropertyType publishedProperty)
        {
            if (IsNestedContentProperty(publishedProperty) == false)
            {
                return false;
            }

            var preValueCollection = NestedContentHelper.GetPreValuesCollectionByDataTypeId(publishedProperty.DataTypeId);
            var preValueDictionary = preValueCollection.PreValuesAsDictionary;

            int minItems, maxItems;
            return preValueDictionary.ContainsKey("minItems") &&
                   int.TryParse(preValueDictionary["minItems"].Value, out minItems) && minItems == 1
                   && preValueDictionary.ContainsKey("maxItems") &&
                   int.TryParse(preValueDictionary["maxItems"].Value, out maxItems) && maxItems == 1;
        }

        public static IEnumerable<IPublishedContent> ConvertPropertyToNestedContent(PublishedPropertyType propertyType, object source)
        {
            using (DisposableTimer.DebugDuration<PublishedPropertyType>(string.Format("ConvertPropertyToNestedContent ({0})", propertyType.DataTypeId)))
            {
                if (source != null && !source.ToString().IsNullOrWhiteSpace())
                {
                    var rawValue = JsonConvert.DeserializeObject<List<object>>(source.ToString());
                    var processedValue = new List<IPublishedContentWithKey>();

                    for (var i = 0; i < rawValue.Count; i++)
                    {
                        var item = (JObject)rawValue[i];
                        
                        var contentTypeAlias = NestedContentHelper.GetContentTypeAliasFromJson(item);
                        if (string.IsNullOrEmpty(contentTypeAlias))
                        {
                            continue;
                        }

                        var publishedContentType = PublishedContentType.Get(PublishedItemType.Content, contentTypeAlias);
                        if (publishedContentType == null)
                        {
                            continue;
                        }

                        var propValues = item.ToObject<Dictionary<string, object>>();
                        var properties = new List<IPublishedProperty>();

                        foreach (var jProp in propValues)
                        {
                            var propType = publishedContentType.GetPropertyType(jProp.Key);
                            if (propType != null)
                            {
                                properties.Add(new DetachedPublishedProperty(propType, jProp.Value));
                            }
                        }

                        // Parse out the key manually
                        object keyObj = null;
                        if (propValues.TryGetValue("key", out keyObj))
                        {
                            // Do nothing, we just want to parse out the name if we can
                        }

                        // Parse out the name manually
                        object nameObj = null;
                        if (propValues.TryGetValue("name", out nameObj))
                        {
                            // Do nothing, we just want to parse out the name if we can
                        }

                        // Get the current request node we are embedded in
                        var pcr = UmbracoContext.Current.PublishedContentRequest;
                        var containerNode = pcr != null && pcr.HasPublishedContent ? pcr.PublishedContent : null;

                        processedValue.Add(new NestedPublishedContent(
                            keyObj == null ? Guid.Empty : Guid.Parse(keyObj.ToString()),
                            nameObj == null ? null : nameObj.ToString(),
                            publishedContentType,
                            properties.ToArray(),
                            containerNode,
                            i));
                    }

                    return processedValue;
                }
            }

            return null;
        }
    }
}

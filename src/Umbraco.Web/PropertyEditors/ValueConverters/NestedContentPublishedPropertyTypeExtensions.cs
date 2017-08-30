using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    internal static class NestedContentPublishedPropertyTypeExtensions
    {
        public static bool IsNestedContentProperty(this PublishedPropertyType publishedProperty)
        {
            return publishedProperty.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.NestedContentAlias);
        }

        public static bool IsSingleNestedContentProperty(this PublishedPropertyType publishedProperty)
        {
            if (!publishedProperty.IsNestedContentProperty())
            {
                return false;
            }

            var preValueCollection = NestedContentHelper.GetPreValuesCollectionByDataTypeId(publishedProperty.DataTypeId);
            var preValueDictionary = preValueCollection.PreValuesAsDictionary.ToDictionary(x => x.Key, x => x.Value.Value);

            int minItems, maxItems;
            return preValueDictionary.ContainsKey("minItems") &&
                   int.TryParse(preValueDictionary["minItems"], out minItems) && minItems == 1
                   && preValueDictionary.ContainsKey("maxItems") &&
                   int.TryParse(preValueDictionary["maxItems"], out maxItems) && maxItems == 1;
        }

        public static object ConvertPropertyToNestedContent(this PublishedPropertyType propertyType, object source, bool preview)
        {
            using (DisposableTimer.DebugDuration<PublishedPropertyType>(string.Format("ConvertPropertyToNestedContent ({0})", propertyType.DataTypeId)))
            {
                if (source != null && !source.ToString().IsNullOrWhiteSpace())
                {
                    var rawValue = JsonConvert.DeserializeObject<List<object>>(source.ToString());
                    var processedValue = new List<IPublishedContent>();

                    var preValueCollection = NestedContentHelper.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeId);
                    var preValueDictionary = preValueCollection.PreValuesAsDictionary.ToDictionary(x => x.Key, x => x.Value.Value);

                    for (var i = 0; i < rawValue.Count; i++)
                    {
                        var item = (JObject)rawValue[i];

                        // Convert from old style (v.0.1.1) data format if necessary
                        // - Please note: This call has virtually no impact on rendering performance for new style (>v0.1.1).
                        //                Even so, this should be removed eventually, when it's safe to assume that there is
                        //                no longer any need for conversion.
                        NestedContentHelper.ConvertItemValueFromV011(item, propertyType.DataTypeId, ref preValueCollection);

                        var contentTypeAlias = NestedContentHelper.GetContentTypeAliasFromItem(item);
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
                                properties.Add(new DetachedPublishedProperty(propType, jProp.Value, preview));
                            }
                        }

                        // Parse out the name manually
                        object nameObj = null;
                        if (propValues.TryGetValue("name", out nameObj))
                        {
                            // Do nothing, we just want to parse out the name if we can
                        }

                        object keyObj;
                        var key = Guid.Empty;
                        if (propValues.TryGetValue("key", out keyObj))
                        {
                            key = Guid.Parse(keyObj.ToString());
                        }

                        // Get the current request node we are embedded in
                        var pcr = UmbracoContext.Current == null ? null : UmbracoContext.Current.PublishedContentRequest;
                        var containerNode = pcr != null && pcr.HasPublishedContent ? pcr.PublishedContent : null;

                        // Create the model based on our implementation of IPublishedContent
                        IPublishedContent content = new DetachedPublishedContent(
                            key,
                            nameObj == null ? null : nameObj.ToString(),
                            publishedContentType,
                            properties.ToArray(),
                            containerNode,
                            i,
                            preview);

                        if (PublishedContentModelFactoryResolver.HasCurrent && PublishedContentModelFactoryResolver.Current.HasValue)
                        {
                            // Let the current model factory create a typed model to wrap our model
                            content = PublishedContentModelFactoryResolver.Current.Factory.CreateModel(content);
                        }

                        // Add the (typed) model as a result
                        processedValue.Add(content);
                    }

                    if (propertyType.IsSingleNestedContentProperty())
                    {
                        return processedValue.FirstOrDefault();
                    }

                    return processedValue;
                }
            }

            return null;
        }
    }
}

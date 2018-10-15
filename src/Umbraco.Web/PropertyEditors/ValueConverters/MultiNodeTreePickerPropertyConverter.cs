// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiNodeTreePickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   The multi node tree picker property editor value converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.Extensions;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{

    /// <summary>
    /// The multi node tree picker property editor value converter.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(MustBeStringValueConverter))]
    [PropertyValueType(typeof(IEnumerable<IPublishedContent>))]
    [PropertyValueCache(PropertyCacheValue.Object, PropertyCacheLevel.ContentCache)]
    [PropertyValueCache(PropertyCacheValue.Source, PropertyCacheLevel.Content)]
    [PropertyValueCache(PropertyCacheValue.XPath, PropertyCacheLevel.Content)]
    public class MultiNodeTreePickerPropertyConverter : PropertyValueConverterBase
    {
        /// <summary>
        /// The properties to exclude.
        /// </summary>
        private static readonly List<string> PropertiesToExclude = new List<string>()
        {
            Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
            Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture)
        };

        /// <summary>
        /// Checks if this converter can convert the property editor and registers if it can.
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultiNodeTreePicker2Alias))
                return true;

            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
            {
                return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultiNodeTreePickerAlias);
            }
            return false;
        }

        /// <summary>
        /// Convert the raw string into a nodeId integer array
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <param name="source">
        /// The value of the property
        /// </param>
        /// <param name="preview">
        /// The preview.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultiNodeTreePickerAlias))
            {
                var nodeIds = source.ToString()
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToArray();
                return nodeIds;
            }
            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultiNodeTreePicker2Alias))
            {
                var nodeIds = source.ToString()
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(Udi.Parse)
                    .ToArray();
                return nodeIds;
            }
            return null;
        }

        /// <summary>
        /// Convert the source nodeId into a IEnumerable of IPublishedContent (or DynamicPublishedContent)
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <param name="source">
        /// The value of the property
        /// </param>
        /// <param name="preview">
        /// The preview.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            //TODO: Inject an UmbracoHelper and create a GetUmbracoHelper method based on either injected or singleton
            if (UmbracoContext.Current == null) return source;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultiNodeTreePickerAlias))
            {
                var nodeIds = (int[])source;

                if ((propertyType.PropertyTypeAlias != null && PropertiesToExclude.InvariantContains(propertyType.PropertyTypeAlias)) == false)
                {
                    var multiNodeTreePicker = new List<IPublishedContent>();

                    var objectType = UmbracoObjectTypes.Unknown;

                    foreach (var nodeId in nodeIds)
                    {
                        var multiNodeTreePickerItem =
                            GetPublishedContent(nodeId, ref objectType, UmbracoObjectTypes.Document, umbHelper.TypedContent)
                            ?? GetPublishedContent(nodeId, ref objectType, UmbracoObjectTypes.Media, umbHelper.TypedMedia)
                            ?? GetPublishedContent(nodeId, ref objectType, UmbracoObjectTypes.Member, umbHelper.TypedMember);

                        if (multiNodeTreePickerItem != null)
                        {
                            multiNodeTreePicker.Add(multiNodeTreePickerItem);
                        }
                    }

                    return multiNodeTreePicker;
                }

                // return the first nodeId as this is one of the excluded properties that expects a single id
                return nodeIds.FirstOrDefault();
            }

            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultiNodeTreePicker2Alias))
            {
                var udis = (Udi[])source;

                if ((propertyType.PropertyTypeAlias != null && PropertiesToExclude.InvariantContains(propertyType.PropertyTypeAlias)) == false)
                {
                    var multiNodeTreePicker = new List<IPublishedContent>();

                    var objectType = UmbracoObjectTypes.Unknown;
                    IPublishedContent multiNodeTreePickerItem = null;

                    foreach (var udi in udis)
                    {
                        switch (udi.EntityType)
                        {
                            case Constants.UdiEntityType.Document:
                                multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Document, umbHelper.TypedContent);
                                break;
                            case Constants.UdiEntityType.Media:
                                multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Media, umbHelper.TypedMedia);
                                break;
                            case Constants.UdiEntityType.Member:
                                multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Member, umbHelper.TypedMember);
                                break;
                        }

                        if (multiNodeTreePickerItem != null)
                        {
                            multiNodeTreePicker.Add(multiNodeTreePickerItem);
                        }
                    }

                    return multiNodeTreePicker;
                }

                // return the first nodeId as this is one of the excluded properties that expects a single id
                return udis.FirstOrDefault();
            }
            return source;
        }

        /// <summary>
        /// Attempt to get an IPublishedContent instance based on ID and content type
        /// </summary>
        /// <param name="nodeId">The content node ID</param>
        /// <param name="actualType">The type of content being requested</param>
        /// <param name="expectedType">The type of content expected/supported by <paramref name="contentFetcher"/></param>
        /// <param name="contentFetcher">A function to fetch content of type <paramref name="expectedType"/></param>
        /// <returns>The requested content, or null if either it does not exist or <paramref name="actualType"/> does not match <paramref name="expectedType"/></returns>
        private IPublishedContent GetPublishedContent<T>(T nodeId, ref UmbracoObjectTypes actualType, UmbracoObjectTypes expectedType, Func<T, IPublishedContent> contentFetcher)
        {
            // is the actual type supported by the content fetcher?
            if (actualType != UmbracoObjectTypes.Unknown && actualType != expectedType)
            {
                // no, return null
                return null;
            }

            // attempt to get the content
            var content = contentFetcher(nodeId);
            if (content != null)
            {
                // if we found the content, assign the expected type to the actual type so we don't have to keep looking for other types of content
                actualType = expectedType;
            }
            return content;
        }
    }
}

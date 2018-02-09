// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiNodeTreePickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   The multi node tree picker property editor value converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;

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
        private readonly IDataTypeService _dataTypeService;

        //TODO: Remove this ctor in v8 since the other one will use IoC
        public MultiNodeTreePickerPropertyConverter()
            : this(ApplicationContext.Current.Services.DataTypeService)
        { }

        public MultiNodeTreePickerPropertyConverter(IDataTypeService dataTypeService)
            : base()
        {
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            _dataTypeService = dataTypeService;
        }

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

            Func<object, IPublishedContent> contentFetcher;

            switch (GetPublishedContentType(propertyType.DataTypeId))
            {
                case PublishedItemType.Media:
                    contentFetcher = umbHelper.TypedMedia;
                    break;

                case PublishedItemType.Member:
                    contentFetcher = umbHelper.TypedMember;
                    break;

                case PublishedItemType.Content:
                default:
                    contentFetcher = umbHelper.TypedContent;
                    break;
            }

            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultiNodeTreePickerAlias))
            {
                var nodeIds = (int[])source;

                if ((propertyType.PropertyTypeAlias != null && PropertiesToExclude.InvariantContains(propertyType.PropertyTypeAlias)) == false)
                {
                    var multiNodeTreePicker = new List<IPublishedContent>();

                    foreach (var nodeId in nodeIds)
                    {
                        var multiNodeTreePickerItem = contentFetcher(nodeId);

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

                    foreach (var udi in udis)
                    {
                        var multiNodeTreePickerItem = contentFetcher(udi);
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

        private PublishedItemType GetPublishedContentType(int dataTypeId)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher
            // e.g. https://github.com/umbraco/Umbraco-CMS/blob/dev-v7/src/Umbraco.Web/Cache/DataTypeCacheRefresher.cs#L116-L119

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var preValue = _dataTypeService
                    .GetPreValuesCollectionByDataTypeId(id)
                    .PreValuesAsDictionary
                    .FirstOrDefault(x => string.Equals(x.Key, "startNode", StringComparison.InvariantCultureIgnoreCase))
                    .Value;

                if (preValue != null && string.IsNullOrWhiteSpace(preValue.Value) == false)
                {
                    var data = JsonConvert.DeserializeAnonymousType(preValue.Value, new { type = string.Empty });
                    if (data != null)
                    {
                        switch (data.type.ToUpperInvariant())
                        {
                            case "MEDIA":
                                return PublishedItemType.Media;

                            case "MEMBER":
                                return PublishedItemType.Member;

                            case "CONTENT":
                            default:
                                return PublishedItemType.Content;
                        }
                    }
                }

                return PublishedItemType.Content;
            });
        }

        private static readonly ConcurrentDictionary<int, PublishedItemType> Storages = new ConcurrentDictionary<int, PublishedItemType>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}

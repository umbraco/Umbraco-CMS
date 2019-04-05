// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyMediaPickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   The multiple media picker property editor converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The multiple media picker and double legacy media picker property value converter, should be deleted for v8
    /// </summary>
    [DefaultPropertyValueConverter(typeof(MustBeStringValueConverter))]
    public class LegacyMediaPickerPropertyConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        private readonly IDataTypeService _dataTypeService;

        //TODO: Remove this ctor in v8 since the other one will use IoC
        public LegacyMediaPickerPropertyConverter()
            : this(ApplicationContext.Current.Services.DataTypeService)
        {
        }

        public LegacyMediaPickerPropertyConverter(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException("dataTypeService");
        }

        /// <summary>
        /// Checks if this converter can convert the property editor and registers if it can.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters && propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultipleMediaPickerAlias))
            {
                return true;
            }

            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters && propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MediaPickerAlias))
            {
                // this is the double legacy media picker, it can pick only single media items
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert the raw string into a nodeId integer array or a single integer
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
            if (source == null)
                return null;

            if (IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias))
            {
                var nodeIds =
                    source.ToString()
                        .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray();
                return nodeIds;
            }

            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
            {
                return attemptConvertInt.Result;
            }
            else
            {
                var nodeIds =
                   source.ToString()
                       .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(int.Parse)
                       .ToArray();

                if (nodeIds.Length > 0)
                {
                    var error =
                        string.Format(
                            "Data type \"{0}\" is not set to allow multiple items but appears to contain multiple items, check the setting and save the data type again",
                            ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(
                                propertyType.DataTypeId).Name);

                    LogHelper.Warn<LegacyMediaPickerPropertyConverter>(error);
                    throw new Exception(error);
                }
            }

            return null;
        }

        /// <summary>
        /// Convert the source nodeId into a IPublishedContent or IEnumerable of IPublishedContent (or DynamicPublishedContent) depending on data type setting
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
                return null;

            var umbracoContext = UmbracoContext.Current;
            if (umbracoContext == null)
                return null;

            if (IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias))
            {
                var nodeIds = (int[])source;
                var multiMediaPicker = Enumerable.Empty<IPublishedContent>();
                if (nodeIds.Length > 0)
                {
                    multiMediaPicker = nodeIds.Select(umbracoContext.MediaCache.GetById).Where(x => x != null);
                }

                // in v8 should return multiNodeTreePickerEnumerable but for v7 need to return as PublishedContentEnumerable so that string can be returned for legacy compatibility
                return new PublishedContentEnumerable(multiMediaPicker);
            }

            // single value picker
            var nodeId = (int)source;

            return umbracoContext.MediaCache.GetById(nodeId);
        }

        /// <summary>
        /// The get property cache level.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <param name="cacheValue">
        /// The cache value.
        /// </param>
        /// <returns>
        /// The <see cref="PropertyCacheLevel"/>.
        /// </returns>
        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            PropertyCacheLevel returnLevel;
            switch (cacheValue)
            {
                case PropertyCacheValue.Object:
                    returnLevel = PropertyCacheLevel.ContentCache;
                    break;
                case PropertyCacheValue.Source:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                case PropertyCacheValue.XPath:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                default:
                    returnLevel = PropertyCacheLevel.None;
                    break;
            }

            return returnLevel;
        }

        /// <summary>
        /// The get property value type.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias) ? typeof(IEnumerable<IPublishedContent>) : typeof(IPublishedContent);
        }

        /// <summary>
        /// The is multiple data type.
        /// </summary>
        /// <param name="dataTypeId">
        /// The data type id.
        /// </param>
        /// <param name="propertyEditorAlias"></param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsMultipleDataType(int dataTypeId, string propertyEditorAlias)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var preVals = _dataTypeService.GetPreValuesCollectionByDataTypeId(id).PreValuesAsDictionary;

                if (preVals.ContainsKey("multiPicker"))
                {
                    var preValue = preVals
                        .FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase))
                        .Value;

                    return preValue != null && preValue.Value.TryConvertTo<bool>().Result;
                }

                //in some odd cases, the pre-values in the db won't exist but their default pre-values contain this key so check there 
                var propertyEditor = PropertyEditorResolver.Current.GetByAlias(propertyEditorAlias);
                if (propertyEditor != null)
                {
                    var preValue = propertyEditor.DefaultPreValues
                        .FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase))
                        .Value;

                    return preValue != null && preValue.TryConvertTo<bool>().Result;
                }

                return false;
            });
        }

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}

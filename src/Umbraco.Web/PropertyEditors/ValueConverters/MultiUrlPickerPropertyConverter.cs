using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Web.Models;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class MultiUrlPickerPropertyConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        private readonly IDataTypeService _dataTypeService;

        public MultiUrlPickerPropertyConverter(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException("dataTypeService");
        }

        //TODO: Remove this ctor in v8 since the other one will use IoC
        public MultiUrlPickerPropertyConverter() : this(ApplicationContext.Current.Services.DataTypeService)
        {
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultiUrlPickerAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var jsonSource = source?.ToString();
            if (jsonSource == null)
                return null;

            if (jsonSource.TrimStart().StartsWith("[") == false)
                return null;

            try
            {
                return JArray.Parse(jsonSource);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MultiUrlPickerPropertyConverter>("Error parsing JSON", ex);
            }

            return null;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            var isMultiple = IsMultipleDataType(propertyType.DataTypeId, out var maxNumber);
            if (source == null)
                return isMultiple
                    ? Enumerable.Empty<Link>()
                    : null;

            var umbracoContext = UmbracoContext.Current;
            if (umbracoContext == null)
                return source;

            var links = new List<Link>();
            var dtos = ((JArray)source).ToObject<IEnumerable<MultiUrlPickerPropertyEditor.LinkDto>>();

            foreach (var dto in dtos)
            {
                var type = LinkType.External;
                var url = dto.Url;
                IPublishedContent content = null;

                if (dto.Udi != null)
                {
                    type = dto.Udi.EntityType == Constants.UdiEntityType.Media
                        ? LinkType.Media
                        : LinkType.Content;

                    if (type == LinkType.Media)
                    {
                        content = umbracoContext.MediaCache.GetById(dto.Udi);
                    }
                    else
                    {
                        content = umbracoContext.ContentCache.GetById(dto.Udi);
                    }

                    if (content == null)
                        continue;

                    url = content.Url;
                }

                var link = new Link
                {
                    Name = dto.Name,
                    Target = dto.Target,
                    Type = type,
                    Udi = dto.Udi,
                    Content = content,
                    Url = url + dto.QueryString,
                };

                links.Add(link);
            }

            if (isMultiple == false)
                return links.FirstOrDefault();

            if (maxNumber > 0)
                return links.Take(maxNumber);

            return links;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return IsMultipleDataType(propertyType.DataTypeId, out var maxNumber)
                ? typeof(IEnumerable<Link>)
                : typeof(Link);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            switch (cacheValue)
            {
                case PropertyCacheValue.Source:
                    return PropertyCacheLevel.Content;
                case PropertyCacheValue.Object:
                case PropertyCacheValue.XPath:
                    return PropertyCacheLevel.ContentCache;
            }

            return PropertyCacheLevel.None;
        }

        private bool IsMultipleDataType(int dataTypeId, out int maxNumber)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher

            maxNumber = Storages.GetOrAdd(dataTypeId, id =>
            {
                var preValues = _dataTypeService.GetPreValuesCollectionByDataTypeId(id).PreValuesAsDictionary;

                return preValues.TryGetValue("maxNumber", out var maxNumberPreValue)
                    ? maxNumberPreValue.Value.TryConvertTo<int>().Result
                    : 0;
            });

            return maxNumber != 1;
        }

        private static readonly ConcurrentDictionary<int, int> Storages = new ConcurrentDictionary<int, int>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}

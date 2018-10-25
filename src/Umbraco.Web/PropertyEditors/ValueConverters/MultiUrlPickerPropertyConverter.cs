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
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            _dataTypeService = dataTypeService;
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
            if (source == null)
            {
                return null;
            }

            if (source.ToString().Trim().StartsWith("["))
            {
                try
                {
                    return JArray.Parse(source.ToString());
                }
                catch (Exception ex)
                {
                    LogHelper.Error<MultiUrlPickerPropertyConverter>("Error parsing JSON", ex);
                }
            }
            return null;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            int maxNumber;
            bool isMultiple = IsMultipleDataType(propertyType.DataTypeId, out maxNumber);
            if (source == null)
            {
                return isMultiple ? Enumerable.Empty<Link>() : null;
            }

            //TODO: Inject an UmbracoHelper and create a GetUmbracoHelper method based on either injected or singleton
            if (UmbracoContext.Current == null) return source;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            var links = new List<Link>();
            var dtos = ((JArray) source).ToObject<IEnumerable<MultiUrlPickerPropertyEditor.LinkDto>>();

            foreach (var dto in dtos)
            {
                LinkType type = LinkType.External;
                string url = dto.Url;
                if (dto.Udi != null)
                {
                    type = dto.Udi.EntityType == Constants.UdiEntityType.Media
                        ? LinkType.Media
                        : LinkType.Content;

                    if (type == LinkType.Media)
                    {
                        var media = umbHelper.TypedMedia(dto.Udi);
                        if (media == null)
                        {
                            continue;
                        }
                        url = media.Url;
                    }
                    else
                    {
                        var content = umbHelper.TypedContent(dto.Udi);
                        if (content == null)
                        {
                            continue;
                        }
                        url = content.Url;
                    }
                }

                var link = new Link
                {
                    Name = dto.Name,
                    Target = dto.Target,
                    Type = type,
                    Udi = dto.Udi,
                    Url = url + dto.QueryString,
                };
                links.Add(link);
            }

            if (isMultiple == false) return links.FirstOrDefault();
            if (maxNumber > 0) return links.Take(maxNumber);
            return links;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            int maxNumber;
            if (IsMultipleDataType(propertyType.DataTypeId, out maxNumber))
            {
                return typeof(IEnumerable<Link>);
            }
            return typeof(Link);
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
                var preVals = _dataTypeService.GetPreValuesCollectionByDataTypeId(id).PreValuesAsDictionary;

                PreValue maxNumberPreValue;
                if (preVals.TryGetValue("maxNumber", out maxNumberPreValue))
                {
                    return maxNumberPreValue.Value.TryConvertTo<int>().Result;
                }

                return 0;
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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <inheritdoc />
    /// <summary>
    /// Provides an implementation for <see cref="T:Umbraco.Core.PropertyEditors.IPropertyValueConverter" /> for nested content.
    /// </summary>
    public class NestedContentManyValueConverter : NestedContentValueConverterBase
    {
        private readonly ConcurrentDictionary<Type, Func<object>> _listCtors = new ConcurrentDictionary<Type, Func<object>>();
        private readonly ProfilingLogger _proflog;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedContentManyValueConverter"/> class.
        /// </summary>
        public NestedContentManyValueConverter(IFacadeAccessor facadeAccessor, IFacadeService facadeService, IPublishedModelFactory publishedModelFactory, ProfilingLogger proflog)
            : base(facadeAccessor, facadeService, publishedModelFactory)
        {
            _proflog = proflog;
        }

        /// <inheritdoc />
        public override bool IsConverter(PublishedPropertyType propertyType)
            => IsNestedMany(propertyType);

        /// <inheritdoc />
        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            var preValueCollection = NestedContentHelper.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeId);
            var contentTypes = preValueCollection.PreValuesAsDictionary["contentTypes"].Value;
            return contentTypes.Contains(",")
                ? typeof (IEnumerable<IPublishedElement>)
                : typeof (IEnumerable<>).MakeGenericType(ModelType.For(contentTypes));
        }

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Content;

        /// <inheritdoc />
        public override object ConvertSourceToInter(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            return source?.ToString();
        }

        /// <inheritdoc />
        public override object ConvertInterToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            using (_proflog.DebugDuration<PublishedPropertyType>($"ConvertPropertyToNestedContent ({propertyType.DataTypeId})"))
            {
                var value = (string)inter;
                if (string.IsNullOrWhiteSpace(value)) return null;

                var objects = JsonConvert.DeserializeObject<List<JObject>>(value);
                if (objects.Count == 0)
                    return Enumerable.Empty<IPublishedElement>();

                // fixme do NOT do it here!
                var preValueCollection = NestedContentHelper.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeId);
                var contentTypes = preValueCollection.PreValuesAsDictionary["contentTypes"].Value;
                IList elements;
                if (contentTypes.Contains(","))
                {
                    elements = new List<IPublishedElement>();
                }
                else if (PublishedModelFactory.ModelTypeMap.TryGetValue(contentTypes, out var type))
                {
                    var ctor = _listCtors.GetOrAdd(type, t =>
                    {
                        var listType = typeof(List<>).MakeGenericType(t);
                        return ReflectionUtilities.GetCtor(listType);
                    });

                    elements = (IList) ctor();
                }
                else
                {
                    // should we throw instead?
                    elements = new List<IPublishedElement>();
                }

                foreach (var sourceObject in objects)
                {
                    var element = ConvertToElement(sourceObject, referenceCacheLevel, preview);
                    if (element != null)
                        elements.Add(element);
                }

                return elements;
            }
        }
    }
}

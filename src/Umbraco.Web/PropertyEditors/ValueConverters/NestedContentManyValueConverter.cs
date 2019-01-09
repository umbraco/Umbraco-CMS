using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly IProfilingLogger _proflog;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedContentManyValueConverter"/> class.
        /// </summary>
        public NestedContentManyValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IPublishedModelFactory publishedModelFactory, IProfilingLogger proflog)
            : base(publishedSnapshotAccessor, publishedModelFactory)
        {
            _proflog = proflog;
        }

        /// <inheritdoc />
        public override bool IsConverter(PublishedPropertyType propertyType)
            => IsNestedMany(propertyType);

        /// <inheritdoc />
        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            var contentTypes = propertyType.DataType.ConfigurationAs<NestedContentConfiguration>().ContentTypes;
            return contentTypes.Length > 1
                ? typeof (IEnumerable<IPublishedElement>)
                : typeof (IEnumerable<>).MakeGenericType(ModelType.For(contentTypes[0].Alias));
        }

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        /// <inheritdoc />
        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            return source?.ToString();
        }

        /// <inheritdoc />
        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            using (_proflog.DebugDuration<PublishedPropertyType>($"ConvertPropertyToNestedContent ({propertyType.DataType.Id})"))
            {
                var configuration = propertyType.DataType.ConfigurationAs<NestedContentConfiguration>();
                var contentTypes = configuration.ContentTypes;
                var elements = contentTypes.Length > 1
                    ? new List<IPublishedElement>()
                    : PublishedModelFactory.CreateModelList(contentTypes[0].Alias);

                var value = (string)inter;
                if (string.IsNullOrWhiteSpace(value)) return elements;

                var objects = JsonConvert.DeserializeObject<List<JObject>>(value);
                if (objects.Count == 0) return elements;

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

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <inheritdoc />
    /// <summary>
    /// Provides an implementation for <see cref="T:Umbraco.Core.PropertyEditors.IPropertyValueConverter" /> for nested content.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class NestedContentSingleValueConverter : NestedContentValueConverterBase
    {
        private readonly IProfilingLogger _proflog;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedContentSingleValueConverter"/> class.
        /// </summary>
        public NestedContentSingleValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IPublishedModelFactory publishedModelFactory, IProfilingLogger proflog)
            : base(publishedSnapshotAccessor, publishedModelFactory)
        {
            _proflog = proflog;
        }

        /// <inheritdoc />
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => IsNestedSingle(propertyType);

        /// <inheritdoc />
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        {
            var contentTypes = propertyType.DataType.ConfigurationAs<NestedContentConfiguration>().ContentTypes;
            return contentTypes.Length > 1
                ? typeof(IPublishedElement)
                : ModelType.For(contentTypes[0].Alias);
        }

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        /// <inheritdoc />
        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            return source?.ToString();
        }

        /// <inheritdoc />
        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            using (_proflog.DebugDuration<NestedContentSingleValueConverter>($"ConvertPropertyToNestedContent ({propertyType.DataType.Id})"))
            {
                var value = (string)inter;
                if (string.IsNullOrWhiteSpace(value)) return null;

                var objects = JsonConvert.DeserializeObject<List<JObject>>(value);
                if (objects.Count == 0)
                    return null;
                if (objects.Count > 1)
                    throw new InvalidOperationException();

                return ConvertToElement(objects[0], referenceCacheLevel, preview);
            }
        }
    }
}

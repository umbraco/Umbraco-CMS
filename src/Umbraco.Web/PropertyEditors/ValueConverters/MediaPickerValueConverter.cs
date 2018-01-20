using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The media picker property value converter.
    /// </summary>
    [DefaultPropertyValueConverter]
    public class MediaPickerValueConverter : PropertyValueConverterBase
    {
        private readonly ServiceContext _services;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MediaPickerValueConverter(ServiceContext services, PropertyEditorCollection propertyEditors, IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _propertyEditors = propertyEditors ?? throw new ArgumentException(nameof(propertyEditors));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MediaPicker2);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => IsMultipleDataType(propertyType.DataType.Id, propertyType.EditorAlias)
                ? typeof (IEnumerable<IPublishedContent>)
                : typeof (IPublishedContent);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        private bool IsMultipleDataType(int dataTypeId, string propertyEditorAlias)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var preVals = _services.DataTypeService.GetPreValuesCollectionByDataTypeId(id).PreValuesAsDictionary;

                if (preVals.ContainsKey("multiPicker"))
                {
                    var preValue = preVals
                        .FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase))
                        .Value;

                    return preValue != null && preValue.Value.TryConvertTo<bool>().Result;
                }

                //in some odd cases, the pre-values in the db won't exist but their default pre-values contain this key so check there
                if (_propertyEditors.TryGet(propertyEditorAlias, out PropertyEditor propertyEditor))
                {
                    var preValue = propertyEditor.DefaultPreValues
                        .FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase))
                        .Value;

                    return preValue != null && preValue.TryConvertTo<bool>().Result;
                }

                return false;
            });
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            var nodeIds = source.ToString()
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Udi.Parse)
                .ToArray();
            return nodeIds;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            var udis = (Udi[])source;
            var mediaItems = new List<IPublishedContent>();
            if (udis.Any())
            {
                foreach (var udi in udis)
                {
                    var guidUdi = udi as GuidUdi;
                    if (guidUdi == null) continue;
                    var item = _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(guidUdi.Guid);
                    if (item != null)
                        mediaItems.Add(item);
                }

                if (IsMultipleDataType(propertyType.DataType.Id, propertyType.EditorAlias))
                    return mediaItems;
                return mediaItems.FirstOrDefault();
            }

            return source;
        }

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}

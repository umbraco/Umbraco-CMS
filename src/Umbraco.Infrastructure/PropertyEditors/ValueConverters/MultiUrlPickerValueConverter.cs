// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    public class MultiUrlPickerValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IProfilingLogger _proflog;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedUrlProvider _publishedUrlProvider;

        public MultiUrlPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IProfilingLogger proflog, IJsonSerializer jsonSerializer, IUmbracoContextAccessor umbracoContextAccessor, IPublishedUrlProvider publishedUrlProvider)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _proflog = proflog ?? throw new ArgumentNullException(nameof(proflog));
            _jsonSerializer = jsonSerializer;
            _umbracoContextAccessor = umbracoContextAccessor;
            _publishedUrlProvider = publishedUrlProvider;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType) => Constants.PropertyEditors.Aliases.MultiUrlPicker.Equals(propertyType.EditorAlias);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType) =>
            propertyType.DataType.ConfigurationAs<MultiUrlPickerConfiguration>().MaxNumber == 1 ?
                typeof(Link) :
                typeof(IEnumerable<Link>);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

        public override bool? IsValue(object value, PropertyValueLevel level) => value?.ToString() != "[]";

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview) => source?.ToString();

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            using (_proflog.DebugDuration<MultiUrlPickerValueConverter>($"ConvertPropertyToLinks ({propertyType.DataType.Id})"))
            {
                var maxNumber = propertyType.DataType.ConfigurationAs<MultiUrlPickerConfiguration>().MaxNumber;

                if (string.IsNullOrWhiteSpace(inter?.ToString()))
                {
                    return maxNumber == 1 ? null : Enumerable.Empty<Link>();
                }

                var links = new List<Link>();
                var dtos = _jsonSerializer.Deserialize<IEnumerable<MultiUrlPickerValueEditor.LinkDto>>(inter.ToString());
                var publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
                foreach (var dto in dtos)
                {
                    var type = LinkType.External;
                    var url = dto.Url;

                    if (dto.Udi != null)
                    {
                        type = dto.Udi.EntityType == Constants.UdiEntityType.Media
                            ? LinkType.Media
                            : LinkType.Content;

                        var content = type == LinkType.Media ?
                             publishedSnapshot.Media.GetById(preview, dto.Udi.Guid) :
                             publishedSnapshot.Content.GetById(preview, dto.Udi.Guid);

                        if (content == null || content.ContentType.ItemType == PublishedItemType.Element)
                        {
                            continue;
                        }
                        url = content.Url(_publishedUrlProvider);
                    }

                    links.Add(
                        new Link
                        {
                            Name = dto.Name,
                            Target = dto.Target,
                            Type = type,
                            Udi = dto.Udi,
                            Url = url + dto.QueryString,
                        }
                    );
                }

                if (maxNumber == 1) return links.FirstOrDefault();
                if (maxNumber > 0) return links.Take(maxNumber);
                return links;
            }
        }
    }
}

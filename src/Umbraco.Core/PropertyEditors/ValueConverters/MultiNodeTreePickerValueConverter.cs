using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{

    /// <summary>
    /// The multi node tree picker property editor value converter.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(MustBeStringValueConverter))]
    public class MultiNodeTreePickerValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly IMemberService _memberService;

        private static readonly List<string> PropertiesToExclude = new()
        {
            Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
            Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture)
        };

        public MultiNodeTreePickerValueConverter(
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IPublishedModelFactory publishedModelFactory,
            IMemberService memberService)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _publishedModelFactory = publishedModelFactory;
            _memberService = memberService;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType) =>
            propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        {
            var contentTypeStrings = propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.Filter;
            var contentTypes = contentTypeStrings?.Split(',') ?? Array.Empty<string>();

            if (IsSingleNodePicker(propertyType))
            {
                return contentTypes.Length == 1
                    ? ModelType.For(contentTypes[0])
                    : typeof(IPublishedContent);
            }

            return contentTypes.Length == 1
                ? typeof(IEnumerable<>).MakeGenericType(ModelType.For(contentTypes[0]))
                : typeof(IEnumerable<IPublishedContent>);
        }

        public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            if (propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker))
            {
                var nodeIds = source.ToString()?
                    .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                    .Select(UdiParser.Parse)
                    .ToArray();
                return nodeIds;
            }

            return null;
        }

        public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            var udis = (Udi[])source;
            var isSingleNodePicker = IsSingleNodePicker(propertyType);

            var contentTypeStrings = propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.Filter;
            var contentTypes = contentTypeStrings?.Split(',');

            if (PropertiesToExclude.InvariantContains(propertyType.Alias) == false)
            {
                var multiNodeTreePicker = contentTypes?.Length == 1
                    ? _publishedModelFactory.CreateModelList(contentTypes[0])!
                    : new List<IPublishedContent>();

                var objectType = UmbracoObjectTypes.Unknown;
                var publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
                foreach (var udi in udis)
                {
                    if (udi is not GuidUdi guidUdi)
                    {
                        continue;
                    }

                    IPublishedContent? multiNodeTreePickerItem = null;
                    switch (udi.EntityType)
                    {
                        case Constants.UdiEntityType.Document:
                            multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Document, id => publishedSnapshot.Content?.GetById(guidUdi.Guid));
                            break;
                        case Constants.UdiEntityType.Media:
                            multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Media, id => publishedSnapshot.Media?.GetById(guidUdi.Guid));
                            break;
                        case Constants.UdiEntityType.Member:
                            multiNodeTreePickerItem = GetPublishedContent(udi, ref objectType, UmbracoObjectTypes.Member, id =>
                            {
                                IMember? m = _memberService.GetByKey(guidUdi.Guid);
                                if (m == null)
                                {
                                    return null;
                                }

                                IPublishedContent? member = publishedSnapshot.Members?.Get(m);
                                return member;
                            });
                            break;
                    }

                    if (multiNodeTreePickerItem != null && multiNodeTreePickerItem.ContentType.ItemType != PublishedItemType.Element)
                    {
                        multiNodeTreePicker.Add(multiNodeTreePickerItem);
                        if (isSingleNodePicker)
                        {
                            break;
                        }
                    }
                }

                if (isSingleNodePicker)
                {
                    return multiNodeTreePicker.Count > 0 ? multiNodeTreePicker[0] : null;
                }

                return multiNodeTreePicker;
            }

            // return the first nodeId as this is one of the excluded properties that expects a single id
            return udis.FirstOrDefault();
        }

        /// <summary>
        /// Attempt to get an IPublishedContent instance based on ID and content type
        /// </summary>
        /// <param name="nodeId">The content node ID</param>
        /// <param name="actualType">The type of content being requested</param>
        /// <param name="expectedType">The type of content expected/supported by <paramref name="contentFetcher"/></param>
        /// <param name="contentFetcher">A function to fetch content of type <paramref name="expectedType"/></param>
        /// <returns>The requested content, or null if either it does not exist or <paramref name="actualType"/> does not match <paramref name="expectedType"/></returns>
        private IPublishedContent? GetPublishedContent<T>(T nodeId, ref UmbracoObjectTypes actualType, UmbracoObjectTypes expectedType, Func<T, IPublishedContent?> contentFetcher)
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

        private static bool IsSingleNodePicker(IPublishedPropertyType propertyType) => propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.MaxNumber == 1;
    }
}

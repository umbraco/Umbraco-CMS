using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     The multi node tree picker property editor value converter.
/// </summary>
[DefaultPropertyValueConverter(typeof(MustBeStringValueConverter))]
public class MultiNodeTreePickerValueConverter : PropertyValueConverterBase
{
    private static readonly List<string> PropertiesToExclude = new()
    {
        Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
        Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture),
    };

    private readonly IMemberService _memberService;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public MultiNodeTreePickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberService memberService)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _umbracoContextAccessor = umbracoContextAccessor;
        _memberService = memberService;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => IsSingleNodePicker(propertyType)
            ? typeof(IPublishedContent)
            : typeof(IEnumerable<IPublishedContent>);

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        if (propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker))
        {
            Udi[]? nodeIds = source.ToString()?
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

        // TODO: Inject an UmbracoHelper and create a GetUmbracoHelper method based on either injected or singleton
        if (_umbracoContextAccessor.TryGetUmbracoContext(out _))
        {
            if (propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker))
            {
                var udis = (Udi[])source;
                var isSingleNodePicker = IsSingleNodePicker(propertyType);

                if ((propertyType.Alias != null && PropertiesToExclude.InvariantContains(propertyType.Alias)) == false)
                {
                    var multiNodeTreePicker = new List<IPublishedContent>();

                    UmbracoObjectTypes objectType = UmbracoObjectTypes.Unknown;
                    IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
                    foreach (Udi udi in udis)
                    {
                        if (udi is not GuidUdi guidUdi)
                        {
                            continue;
                        }

                        IPublishedContent? multiNodeTreePickerItem = null;
                        switch (udi.EntityType)
                        {
                            case Constants.UdiEntityType.Document:
                                multiNodeTreePickerItem = GetPublishedContent(
                                    udi,
                                    ref objectType,
                                    UmbracoObjectTypes.Document,
                                    id => publishedSnapshot.Content?.GetById(guidUdi.Guid));
                                break;
                            case Constants.UdiEntityType.Media:
                                multiNodeTreePickerItem = GetPublishedContent(
                                    udi,
                                    ref objectType,
                                    UmbracoObjectTypes.Media,
                                    id => publishedSnapshot.Media?.GetById(guidUdi.Guid));
                                break;
                            case Constants.UdiEntityType.Member:
                                multiNodeTreePickerItem = GetPublishedContent(
                                    udi,
                                    ref objectType,
                                    UmbracoObjectTypes.Member,
                                    id =>
                                    {
                                        IMember? m = _memberService.GetByKey(guidUdi.Guid);
                                        if (m == null)
                                        {
                                            return null;
                                        }

                                        IPublishedContent? member = publishedSnapshot?.Members?.Get(m);
                                        return member;
                                    });
                                break;
                        }

                        if (multiNodeTreePickerItem != null &&
                            multiNodeTreePickerItem.ContentType.ItemType != PublishedItemType.Element)
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
                        return multiNodeTreePicker.FirstOrDefault();
                    }

                    return multiNodeTreePicker;
                }

                // return the first nodeId as this is one of the excluded properties that expects a single id
                return udis.FirstOrDefault();
            }
        }

        return source;
    }

    private static bool IsSingleNodePicker(IPublishedPropertyType propertyType) =>
        propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.MaxNumber == 1;

    /// <summary>
    ///     Attempt to get an IPublishedContent instance based on ID and content type
    /// </summary>
    /// <param name="nodeId">The content node ID</param>
    /// <param name="actualType">The type of content being requested</param>
    /// <param name="expectedType">The type of content expected/supported by <paramref name="contentFetcher" /></param>
    /// <param name="contentFetcher">A function to fetch content of type <paramref name="expectedType" /></param>
    /// <returns>
    ///     The requested content, or null if either it does not exist or <paramref name="actualType" /> does not match
    ///     <paramref name="expectedType" />
    /// </returns>
    private IPublishedContent? GetPublishedContent<T>(T nodeId, ref UmbracoObjectTypes actualType, UmbracoObjectTypes expectedType, Func<T, IPublishedContent?> contentFetcher)
    {
        // is the actual type supported by the content fetcher?
        if (actualType != UmbracoObjectTypes.Unknown && actualType != expectedType)
        {
            // no, return null
            return null;
        }

        // attempt to get the content
        IPublishedContent? content = contentFetcher(nodeId);
        if (content != null)
        {
            // if we found the content, assign the expected type to the actual type so we don't have to keep looking for other types of content
            actualType = expectedType;
        }

        return content;
    }
}

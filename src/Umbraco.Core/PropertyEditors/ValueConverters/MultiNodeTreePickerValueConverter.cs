using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     The multi node tree picker property editor value converter.
/// </summary>
[DefaultPropertyValueConverter(typeof(MustBeStringValueConverter))]
public class MultiNodeTreePickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private static readonly List<string> PropertiesToExclude = new()
    {
        Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
        Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture),
    };

    private readonly IMemberService _memberService;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IApiContentBuilder _apiContentBuilder;
    private readonly IApiMediaBuilder _apiMediaBuilder;

    [Obsolete("Use constructor that takes all parameters, scheduled for removal in V14")]
    public MultiNodeTreePickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberService memberService)
        : this(
            publishedSnapshotAccessor,
            umbracoContextAccessor,
            memberService,
            StaticServiceProvider.Instance.GetRequiredService<IApiContentBuilder>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiMediaBuilder>())
    {
    }

    public MultiNodeTreePickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IUmbracoContextAccessor umbracoContextAccessor,
        IMemberService memberService,
        IApiContentBuilder apiContentBuilder,
        IApiMediaBuilder apiMediaBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _umbracoContextAccessor = umbracoContextAccessor;
        _memberService = memberService;
        _apiContentBuilder = apiContentBuilder;
        _apiMediaBuilder = apiMediaBuilder;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => IsSingleNodePicker(propertyType)
            ? typeof(IPublishedContent)
            : typeof(IEnumerable<IPublishedContent>);

    // the intermediate value of this editor is the picked IPublishedContent items (or item in single picker mode).
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null || propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultiNodeTreePicker) is false)
        {
            return null;
        }

        // NOTE: This guard is likely legacy at this point, but we'll keep it around to be safe (no functional breakage).
        //       None of the other pickers have this kind of defensive guard in place. We can remove it at a later point.
        //       For the time being it causes no harm to leave in place.
        if (_umbracoContextAccessor.TryGetUmbracoContext(out _) is false)
        {
            return source;
        }

        Udi[]? udis = ParseUdis(source);

        return udis is null
            ? null
            : ConvertUdisToIntermediateObject(udis, propertyType);
    }

    private Udi[]? ParseUdis(object? source)
    {
        if (source is null)
        {
            return null;
        }

        return source.ToString()?
            .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(item => UdiParser.TryParse(item, out Udi? udi) ? udi : null)
            .WhereNotNull()
            .ToArray();
    }

    private object? ConvertUdisToIntermediateObject(Udi[] udis, IPublishedPropertyType propertyType)
    {
        if (PropertiesToExclude.InvariantContains(propertyType.Alias))
        {
            // return the first nodeId as this is one of the excluded properties that expects a single id
            return udis.FirstOrDefault();
        }

        var isSingleNodePicker = IsSingleNodePicker(propertyType);

        var multiNodeTreePicker = new List<IPublishedContent>();

        var entityType = GetEntityType(propertyType);

        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
        foreach (Udi udi in udis.Where(u => u.EntityType == entityType))
        {
            if (udi is not GuidUdi guidUdi)
            {
                continue;
            }

            IPublishedContent? multiNodeTreePickerItem = null;
            switch (udi.EntityType)
            {
                case Constants.UdiEntityType.Document:
                    multiNodeTreePickerItem = publishedSnapshot.Content?.GetById(guidUdi.Guid);
                    break;
                case Constants.UdiEntityType.Media:
                    multiNodeTreePickerItem = publishedSnapshot.Media?.GetById(guidUdi.Guid);
                    break;
                case Constants.UdiEntityType.Member:
                    IMember? member = _memberService.GetByKey(guidUdi.Guid);
                    multiNodeTreePickerItem = member is not null
                        ? publishedSnapshot.Members?.Get(member)
                        : null;
                    break;
            }

            if (multiNodeTreePickerItem is not null)
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

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? inter, bool preview)
        => inter;

    // for backwards compat let's convert the intermediate value (either a list of content items or a single content item) to an array of media UDIs
    public override object? ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var entityType = GetEntityType (propertyType);
        return inter is IEnumerable<IPublishedContent> items
            ? items.Select(item => new GuidUdi(entityType, item.Key)).ToArray()
            : inter is IPublishedContent item
                ? new[] { new GuidUdi(entityType, item.Key) }
                : null;
    }

    // the API cache level must be Snapshot in order to facilitate nested field expansion and limiting
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => GetEntityType(propertyType) switch
        {
            Constants.UdiEntityType.Media => typeof(IEnumerable<IApiMedia>),
            Constants.UdiEntityType.Member => typeof(string), // unsupported
            _ => typeof(IEnumerable<IApiContent>)
        };


    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        var entityType = GetEntityType(propertyType);
        if (entityType is Constants.UdiEntityType.Member)
        {
            return "(unsupported)";
        }

        IEnumerable<IApiContent> DefaultValue() => Array.Empty<IApiContent>();

        IPublishedContent[]? interItems = inter is IEnumerable<IPublishedContent> interValues
            ? interValues.ToArray()
            : inter is IPublishedContent interValue
                ? new[] { interValue }
                : null;

        if (interItems is null)
        {
            return DefaultValue();
        }

        return entityType switch
        {
            Constants.UdiEntityType.Document => interItems
                .Where(item => item.ItemType is PublishedItemType.Content)
                .Select(item => _apiContentBuilder.Build(item))
                .WhereNotNull()
                .ToArray(),
            Constants.UdiEntityType.Media => interItems
                .Where(item => item.ItemType is PublishedItemType.Media)
                .Select(item => _apiMediaBuilder.Build(item))
                .WhereNotNull()
                .ToArray(),
            _ => DefaultValue()
        };
    }

    private static bool IsSingleNodePicker(IPublishedPropertyType propertyType) =>
        propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.MaxNumber == 1;

    private static string GetEntityType(IPublishedPropertyType propertyType)
    {
        var entityType = propertyType.DataType.ConfigurationAs<MultiNodePickerConfiguration>()?.TreeSource?.ObjectType;
        if (entityType == "content")
        {
            // TODO Why do MNTP config saves "content" and not "document"?
            entityType = Constants.UdiEntityType.Document;
        }

        return entityType ?? Constants.UdiEntityType.Document;
    }
}

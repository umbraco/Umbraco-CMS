using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for member picker properties.
/// </summary>
[DefaultPropertyValueConverter]
public class MemberPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IMemberService _memberService;
    private readonly IPublishedMemberCache _memberCache;
    private readonly IExternalMemberService _externalMemberService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberPickerValueConverter" /> class.
    /// </summary>
    public MemberPickerValueConverter(
        IMemberService memberService,
        IPublishedMemberCache memberCache,
        IExternalMemberService externalMemberService)
    {
        _memberService = memberService;
        _memberCache = memberCache;
        _externalMemberService = externalMemberService;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberPickerValueConverter" /> class.
    /// </summary>
    /// <param name="memberService">The member service.</param>
    /// <param name="memberCache">The published member cache.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public MemberPickerValueConverter(
        IMemberService memberService,
        IPublishedMemberCache memberCache)
        : this(
            memberService,
            memberCache,
            StaticServiceProvider.Instance.GetRequiredService<IExternalMemberService>())
    {
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.MemberPicker);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IPublishedContent);

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        Attempt<int> attemptConvertInt = source.TryConvertTo<int>();
        if (attemptConvertInt.Success)
        {
            return attemptConvertInt.Result;
        }

        Attempt<Udi> attemptConvertUdi = source.TryConvertTo<Udi>();
        if (attemptConvertUdi.Success)
        {
            return attemptConvertUdi.Result;
        }

        return null;
    }

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        if (source is int id)
        {
            IMember? m = _memberService.GetById(id);
            if (m == null)
            {
                return null;
            }

            IPublishedContent? member = _memberCache.Get(m);
            if (member != null)
            {
                return member;
            }
        }
        else
        {
            if (source is not GuidUdi sourceUdi)
            {
                return null;
            }

            IMember? m = _memberService.GetById(sourceUdi.Guid);
            if (m != null)
            {
                IPublishedContent? member = _memberCache.Get(m);
                if (member != null)
                {
                    return member;
                }
            }

            // Fall back to external member store.
            ExternalMemberIdentity? external = _externalMemberService.GetByKeyAsync(sourceUdi.Guid)
                .GetAwaiter().GetResult();
            if (external != null)
            {
                return new PublishedExternalMember(external);
            }
        }

        return source;
    }

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(string);

    /// <inheritdoc />
    /// <remarks>
    ///     Member picker is unsupported for Delivery API output to avoid leaking member data by accident.
    /// </remarks>
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => "(unsupported)";
}

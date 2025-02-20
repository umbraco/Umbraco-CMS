using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class MemberPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IMemberService _memberService;
    private readonly IPublishedMemberCache _memberCache;

    public MemberPickerValueConverter(
        IMemberService memberService,
        IPublishedMemberCache memberCache)
    {
        _memberService = memberService;
        _memberCache = memberCache;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.MemberPicker);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IPublishedContent);

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

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        IPublishedContent? member;
        if (source is int id)
        {
            IMember? m = _memberService.GetById(id);
            if (m == null)
            {
                return null;
            }

            member = _memberCache.Get(m);
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

            IMember? m = _memberService.GetByKey(sourceUdi.Guid);
            if (m == null)
            {
                return null;
            }

            member = _memberCache.Get(m);

            if (member != null)
            {
                return member;
            }
        }

        return source;
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(string);

    // member picker is unsupported for Delivery API output to avoid leaking member data by accident.
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => "(unsupported)";
}

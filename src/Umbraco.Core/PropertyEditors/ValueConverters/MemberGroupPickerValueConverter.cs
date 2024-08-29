using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class MemberGroupPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IMemberGroupService _memberGroupService;

    public MemberGroupPickerValueConverter(IMemberGroupService memberGroupService) => _memberGroupService = memberGroupService;

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.MemberGroupPicker);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview) => source?.ToString() ?? string.Empty;

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(string[]);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        var memberGroupIds = inter?
            .ToString()?
            .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id, out var memberGroupId) ? memberGroupId : 0)
            .Where(id => id > 0)
            .ToArray();
        if (memberGroupIds == null || memberGroupIds.Length == 0)
        {
            return null;
        }

        IEnumerable<IMemberGroup> memberGroups = _memberGroupService.GetByIds(memberGroupIds);
        return memberGroups.Select(m => m.Name).ToArray();
    }
}

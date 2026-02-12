using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for member group picker properties.
/// </summary>
[DefaultPropertyValueConverter]
public class MemberGroupPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IMemberGroupService _memberGroupService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupPickerValueConverter" /> class.
    /// </summary>
    /// <param name="memberGroupService">The member group service.</param>
    public MemberGroupPickerValueConverter(IMemberGroupService memberGroupService) => _memberGroupService = memberGroupService;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.MemberGroupPicker);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview) => source?.ToString() ?? string.Empty;

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(string[]);

    /// <inheritdoc />
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

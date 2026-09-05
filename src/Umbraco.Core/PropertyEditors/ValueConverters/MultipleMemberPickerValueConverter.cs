// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for multiple member picker properties.
/// </summary>
public class MultipleMemberPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IMemberService _memberService;
    private readonly IPublishedMemberCache _publishedMemberCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleMemberPickerValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="memberService">The member service.</param>
    /// <param name="publishedMemberCache">The published member cache.</param>
    public MultipleMemberPickerValueConverter(
        IJsonSerializer jsonSerializer,
        IMemberService memberService,
        IPublishedMemberCache publishedMemberCache)
    {
        _jsonSerializer = jsonSerializer;
        _memberService = memberService;
        _publishedMemberCache = publishedMemberCache;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultipleMemberPicker);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<IPublishedContent>);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public override bool? IsValue(object? value, PropertyValueLevel level)
        => value is not null && value.ToString() != "[]";

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString();

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        => GetMembers(inter);

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(string);

    /// <inheritdoc />
    /// <remarks>
    ///     Member pickers are unsupported for Delivery API output to avoid leaking member data by accident, exactly as
    ///     <see cref="MemberPickerValueConverter" /> is.
    /// </remarks>
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => "(unsupported)";

    private IPublishedContent[] GetMembers(object? inter)
    {
        var value = inter as string;
        if (value.IsNullOrWhiteSpace())
        {
            return [];
        }

        Guid[]? keys = _jsonSerializer.Deserialize<Guid[]>(value);

        return keys is null
            ? []
            : keys
                .Select(key => _memberService.GetById(key))
                .WhereNotNull()
                .Select(member => _publishedMemberCache.Get(member))
                .WhereNotNull()
                .ToArray();
    }
}

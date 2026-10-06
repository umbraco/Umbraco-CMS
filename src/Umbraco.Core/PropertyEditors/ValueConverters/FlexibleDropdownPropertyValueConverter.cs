// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for dropdown list properties holding any number of values.
/// </summary>
[DefaultPropertyValueConverter]
public class FlexibleDropdownPropertyValueConverter : DropDownPropertyValueConverterBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FlexibleDropdownPropertyValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public FlexibleDropdownPropertyValueConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    protected override bool HoldsMultipleValues => true;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultipleDropDown);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<string>);
}

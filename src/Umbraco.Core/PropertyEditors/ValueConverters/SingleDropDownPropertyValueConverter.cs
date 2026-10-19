// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for dropdown list properties holding a single value.
/// </summary>
[DefaultPropertyValueConverter]
public class SingleDropDownPropertyValueConverter : DropDownPropertyValueConverterBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SingleDropDownPropertyValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public SingleDropDownPropertyValueConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    protected override bool HoldsMultipleValues => false;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.SingleDropDown);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(string);
}

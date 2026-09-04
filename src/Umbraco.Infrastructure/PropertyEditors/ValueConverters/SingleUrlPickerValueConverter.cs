// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Converts the value stored by the single URL picker property editor into a strongly-typed link object that can be
/// easily consumed by application code.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class SingleUrlPickerValueConverter : MultiUrlPickerValueConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleUrlPickerValueConverter"/> class with the specified dependencies.
    /// </summary>
    /// <param name="proflog">The <see cref="IProfilingLogger"/> used for profiling and logging.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for serializing and deserializing JSON data.</param>
    /// <param name="publishedUrlProvider">The <see cref="IPublishedUrlProvider"/> used to provide published URLs.</param>
    /// <param name="apiContentNameProvider">The <see cref="IApiContentNameProvider"/> used to provide API content names.</param>
    /// <param name="apiMediaUrlProvider">The <see cref="IApiMediaUrlProvider"/> used to provide API media URLs.</param>
    /// <param name="apiContentRouteBuilder">The <see cref="IApiContentRouteBuilder"/> used to build API content routes.</param>
    /// <param name="contentCache">The <see cref="IPublishedContentCache"/> used to cache published content.</param>
    /// <param name="mediaCache">The <see cref="IPublishedMediaCache"/> used to cache published media.</param>
    public SingleUrlPickerValueConverter(
        IProfilingLogger proflog,
        IJsonSerializer jsonSerializer,
        IPublishedUrlProvider publishedUrlProvider,
        IApiContentNameProvider apiContentNameProvider,
        IApiMediaUrlProvider apiMediaUrlProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IPublishedContentCache contentCache,
        IPublishedMediaCache mediaCache)
        : base(
            proflog,
            jsonSerializer,
            publishedUrlProvider,
            apiContentNameProvider,
            apiMediaUrlProvider,
            apiContentRouteBuilder,
            contentCache,
            mediaCache)
    {
    }

    /// <inheritdoc />
    protected override bool HoldsMultipleLinks => false;

    /// <summary>
    /// Determines whether this value converter should be used for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type to evaluate.</param>
    /// <returns><c>true</c> if the property type uses the single URL picker editor; otherwise, <c>false</c>.</returns>
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        Constants.PropertyEditors.Aliases.SingleUrlPicker.Equals(propertyType.EditorAlias);

    /// <summary>
    /// Determines the CLR type returned for the property value.
    /// </summary>
    /// <param name="propertyType">The published property type to evaluate.</param>
    /// <returns><see cref="Link" />.</returns>
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(Link);
}

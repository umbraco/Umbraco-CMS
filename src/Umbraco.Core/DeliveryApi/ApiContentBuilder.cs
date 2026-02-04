using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiContentBuilder"/> that builds API content objects for the Delivery API.
/// </summary>
public sealed class ApiContentBuilder : ApiContentBuilderBase<IApiContent>, IApiContentBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentBuilder"/> class.
    /// </summary>
    /// <param name="apiContentNameProvider">The API content name provider.</param>
    /// <param name="apiContentRouteBuilder">The API content route builder.</param>
    /// <param name="outputExpansionStrategyAccessor">The output expansion strategy accessor.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    public ApiContentBuilder(
        IApiContentNameProvider apiContentNameProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
        : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor, variationContextAccessor)
    {
    }

    /// <inheritdoc />
    protected override IApiContent Create(IPublishedContent content, string name, IApiContentRoute route, IDictionary<string, object?> properties)
        => new ApiContent(content.Key, name, content.ContentType.Alias, content.CreateDate, content.CultureDate(VariationContextAccessor), route, properties);
}

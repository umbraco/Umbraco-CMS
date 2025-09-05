using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiContentBuilder : ApiContentBuilderBase<IApiContent>, IApiContentBuilder
{
    public ApiContentBuilder(
        IApiContentNameProvider apiContentNameProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
        : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor, variationContextAccessor)
    {
    }

    protected override IApiContent Create(IPublishedContent content, string name, IApiContentRoute route, IDictionary<string, object?> properties)
        => new ApiContent(content.Key, name, content.ContentType.Alias, content.CreateDate, content.CultureDate(VariationContextAccessor), route, properties);
}

using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiContentBuilder : ApiContentBuilderBase<IApiContent>, IApiContentBuilder
{
    public ApiContentBuilder(IApiContentNameProvider apiContentNameProvider, IApiContentRouteBuilder apiContentRouteBuilder, IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
        : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor)
    {
    }

    protected override IApiContent Create(IPublishedContent content, Guid id, string name, string contentType, IApiContentRoute route, IDictionary<string, object?> properties)
        => new ApiContent(id, name, contentType, route, properties);
}

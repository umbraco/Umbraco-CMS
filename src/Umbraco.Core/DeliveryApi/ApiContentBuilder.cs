using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiContentBuilder : ApiContentBuilderBase<IApiContent>, IApiContentBuilder
{
    public ApiContentBuilder(IApiContentNameProvider apiContentNameProvider, IApiContentRouteBuilder apiContentRouteBuilder, IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
        : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor)
    {
    }

    protected override IApiContent Create(IPublishedContent content, string name, IApiContentRoute route, IDictionary<string, object?> properties)
        => new ApiContent(content.Key, name, content.ContentType.Alias, content.CreateDate, content.UpdateDate, route, properties);
}

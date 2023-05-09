using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiContentResponseBuilder : ApiContentBuilderBase<IApiContentResponse>, IApiContentResponseBuilder
{
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;

    public ApiContentResponseBuilder(IApiContentNameProvider apiContentNameProvider, IApiContentRouteBuilder apiContentRouteBuilder, IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
        : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor)
        => _apiContentRouteBuilder = apiContentRouteBuilder;

    protected override IApiContentResponse Create(IPublishedContent content, Guid id, string name, string contentType, IApiContentRoute route, IDictionary<string, object?> properties)
    {
        var cultures = content.Cultures.Values
            .Where(publishedCultureInfo => publishedCultureInfo.Culture.IsNullOrWhiteSpace() == false) // filter out invariant cultures
            .ToDictionary(
                publishedCultureInfo => publishedCultureInfo.Culture,
                publishedCultureInfo => _apiContentRouteBuilder.Build(content, publishedCultureInfo.Culture));

        return new ApiContentResponse(id, name, contentType, route, properties, cultures);
    }
}

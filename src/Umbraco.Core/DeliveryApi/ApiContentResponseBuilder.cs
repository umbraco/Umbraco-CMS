using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
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
        var routesByCulture = new Dictionary<string, IApiContentRoute>();

        foreach (PublishedCultureInfo publishedCultureInfo in content.Cultures.Values)
        {
            if (publishedCultureInfo.Culture.IsNullOrWhiteSpace())
            {
                // filter out invariant cultures
                continue;
            }

            IApiContentRoute? cultureRoute = _apiContentRouteBuilder.Build(content, publishedCultureInfo.Culture);
            if (cultureRoute == null)
            {
                // content is un-routable in this culture
                continue;
            }

            routesByCulture[publishedCultureInfo.Culture] = cultureRoute;
        }

        return new ApiContentResponse(id, name, contentType, route, properties, routesByCulture);
    }
}

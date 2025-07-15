using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public class ApiContentResponseBuilder : ApiContentBuilderBase<IApiContentResponse>, IApiContentResponseBuilder
{
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly IVariationContextAccessor _variationContextAccessor;

    [Obsolete("Please use the constructor that takes an IVariationContextAccessor instead. Scheduled for removal in V17.")]
    public ApiContentResponseBuilder(
        IApiContentNameProvider apiContentNameProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
        : this(
            apiContentNameProvider,
            apiContentRouteBuilder,
            outputExpansionStrategyAccessor,
            StaticServiceProvider.Instance.CreateInstance<IVariationContextAccessor>())
    {
    }

    public ApiContentResponseBuilder(
        IApiContentNameProvider apiContentNameProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
        : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor)
    {
        _apiContentRouteBuilder = apiContentRouteBuilder;
        _variationContextAccessor = variationContextAccessor;
    }

    protected override IApiContentResponse Create(IPublishedContent content, string name, IApiContentRoute route, IDictionary<string, object?> properties)
    {
        IDictionary<string, IApiContentRoute> cultures = GetCultures(content);
        return new ApiContentResponse(content.Key, name, content.ContentType.Alias, content.CreateDate, content.CultureDate(_variationContextAccessor), route, properties, cultures);
    }

    protected virtual IDictionary<string, IApiContentRoute> GetCultures(IPublishedContent content)
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

        return routesByCulture;
    }
}

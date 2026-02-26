using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiContentResponseBuilder"/> that builds content response objects for the Delivery API.
/// </summary>
public class ApiContentResponseBuilder : ApiContentBuilderBase<IApiContentResponse>, IApiContentResponseBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentResponseBuilder"/> class.
    /// </summary>
    /// <param name="apiContentNameProvider">The API content name provider.</param>
    /// <param name="apiContentRouteBuilder">The API content route builder.</param>
    /// <param name="outputExpansionStrategyAccessor">The output expansion strategy accessor.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    public ApiContentResponseBuilder(
        IApiContentNameProvider apiContentNameProvider,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
        : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor, variationContextAccessor)
    {
    }

    /// <inheritdoc />
    protected override IApiContentResponse Create(IPublishedContent content, string name, IApiContentRoute route, IDictionary<string, object?> properties)
    {
        IDictionary<string, IApiContentRoute> cultures = GetCultures(content);
        return new ApiContentResponse(content.Key, name, content.ContentType.Alias, content.CreateDate, content.CultureDate(VariationContextAccessor), route, properties, cultures);
    }

    /// <summary>
    ///     Gets the available culture routes for the specified content.
    /// </summary>
    /// <param name="content">The published content to get culture routes for.</param>
    /// <returns>A dictionary of culture codes to their corresponding routes.</returns>
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

            IApiContentRoute? cultureRoute = ApiContentRouteBuilder.Build(content, publishedCultureInfo.Culture);
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

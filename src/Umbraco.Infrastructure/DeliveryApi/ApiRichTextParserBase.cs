using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal abstract partial class ApiRichTextParserBase
{
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly IApiMediaUrlProvider _apiMediaUrlProvider;

    protected ApiRichTextParserBase(IApiContentRouteBuilder apiContentRouteBuilder, IApiMediaUrlProvider apiMediaUrlProvider)
    {
        _apiContentRouteBuilder = apiContentRouteBuilder;
        _apiMediaUrlProvider = apiMediaUrlProvider;
    }

    protected void ReplaceLocalLinks(IPublishedSnapshot publishedSnapshot, string href, Action<IApiContentRoute> handleContentRoute, Action<string> handleMediaUrl, Action handleInvalidLink)
    {
        Match match = LocalLinkRegex().Match(href);
        if (match.Success is false)
        {
            return;
        }

        if (UdiParser.TryParse(match.Groups["udi"].Value, out Udi? udi) is false)
        {
            return;
        }

        bool handled = false;
        switch (udi.EntityType)
        {
            case Constants.UdiEntityType.Document:
                IPublishedContent? content = publishedSnapshot.Content?.GetById(udi);
                IApiContentRoute? route = content != null
                    ? _apiContentRouteBuilder.Build(content)
                    : null;
                if (route != null)
                {
                    route.QueryString = match.Groups["query"].Value.NullOrWhiteSpaceAsNull();
                    handled = true;
                    handleContentRoute(route);
                }

                break;
            case Constants.UdiEntityType.Media:
                IPublishedContent? media = publishedSnapshot.Media?.GetById(udi);
                if (media != null)
                {
                    handled = true;
                    handleMediaUrl(_apiMediaUrlProvider.GetUrl(media));
                }

                break;
        }

        if(handled is false)
        {
            handleInvalidLink();
        }
    }

    protected void ReplaceLocalImages(IPublishedSnapshot publishedSnapshot, string udi, Action<string> handleMediaUrl)
    {
        if (UdiParser.TryParse(udi, out Udi? udiValue) is false)
        {
            return;
        }

        IPublishedContent? media = publishedSnapshot.Media?.GetById(udiValue);
        if (media is null)
        {
            return;
        }

        handleMediaUrl(_apiMediaUrlProvider.GetUrl(media));
    }

    [GeneratedRegex("{localLink:(?<udi>umb:.+)}(?<query>[^\"]*)")]
    private static partial Regex LocalLinkRegex();
}

using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

internal abstract partial class ApiRichTextParserBase
{
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly IApiMediaUrlProvider _apiMediaUrlProvider;

    protected const string BlockContentKeyAttribute = "data-content-key";

    protected ApiRichTextParserBase(IApiContentRouteBuilder apiContentRouteBuilder, IApiMediaUrlProvider apiMediaUrlProvider)
    {
        _apiContentRouteBuilder = apiContentRouteBuilder;
        _apiMediaUrlProvider = apiMediaUrlProvider;
    }

    protected void ReplaceLocalLinks(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache, string href, string type, Action<IApiContentRoute> handleContentRoute, Action<string> handleMediaUrl, Action handleInvalidLink)
    {
        ReplaceStatus replaceAttempt = ReplaceLocalLink(contentCache, mediaCache, href, type, handleContentRoute, handleMediaUrl);
        if (replaceAttempt == ReplaceStatus.Success)
        {
            return;
        }

        if (replaceAttempt == ReplaceStatus.InvalidEntityType || ReplaceLegacyLocalLink(contentCache, mediaCache, href, handleContentRoute, handleMediaUrl) == ReplaceStatus.InvalidEntityType)
        {
            handleInvalidLink();
        }
    }

    private ReplaceStatus ReplaceLocalLink(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache, string href, string type, Action<IApiContentRoute> handleContentRoute, Action<string> handleMediaUrl)
    {
        Match match = LocalLinkRegex().Match(href);
        if (match.Success is false)
        {
            return ReplaceStatus.NoMatch;
        }

        if (Guid.TryParse(match.Groups["guid"].Value, out Guid guid) is false)
        {
            return ReplaceStatus.NoMatch;
        }

        var udi = new GuidUdi(type, guid);

        switch (udi.EntityType)
        {
            case Constants.UdiEntityType.Document:
                IPublishedContent? content = contentCache.GetById(guid);
                IApiContentRoute? route = content != null
                    ? _apiContentRouteBuilder.Build(content)
                    : null;
                if (route != null)
                {
                    handleContentRoute(route);
                    return ReplaceStatus.Success;
                }

                break;
            case Constants.UdiEntityType.Media:
                IPublishedContent? media = mediaCache.GetById(guid);
                if (media != null)
                {
                    handleMediaUrl(_apiMediaUrlProvider.GetUrl(media));
                    return ReplaceStatus.Success;
                }

                break;
        }

        return ReplaceStatus.InvalidEntityType;
    }

    private ReplaceStatus ReplaceLegacyLocalLink(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache, string href, Action<IApiContentRoute> handleContentRoute, Action<string> handleMediaUrl)
    {
        Match match = LegacyLocalLinkRegex().Match(href);
        if (match.Success is false)
        {
            return ReplaceStatus.NoMatch;
        }

        if (UdiParser.TryParse(match.Groups["udi"].Value, out Udi? udi) is false)
        {
            return ReplaceStatus.NoMatch;
        }

        // Looking at the old NuCache implementation, Udi's HAD to be GuidUdi's, so we'll assume that here too
        if(udi is not GuidUdi guidUdi)
        {
            return ReplaceStatus.NoMatch;
        }

        switch (udi.EntityType)
        {
            case Constants.UdiEntityType.Document:
                IPublishedContent? content = contentCache.GetById(guidUdi.Guid);
                IApiContentRoute? route = content != null
                    ? _apiContentRouteBuilder.Build(content)
                    : null;
                if (route != null)
                {
                    handleContentRoute(route);
                    return ReplaceStatus.Success;
                }

                break;
            case Constants.UdiEntityType.Media:
                IPublishedContent? media = mediaCache.GetById(guidUdi.Guid);
                if (media != null)
                {
                    handleMediaUrl(_apiMediaUrlProvider.GetUrl(media));
                    return ReplaceStatus.Success;
                }

                break;
        }

        return ReplaceStatus.InvalidEntityType;
    }

    protected void ReplaceLocalImages(IPublishedMediaCache mediaCache, string udi, Action<string> handleMediaUrl)
    {
        if (UdiParser.TryParse(udi, out Udi? udiValue) is false || udiValue is not GuidUdi guidUdi)
        {
            return;
        }

        IPublishedContent? media = mediaCache.GetById(guidUdi.Guid);
        if (media is null)
        {
            return;
        }

        handleMediaUrl(_apiMediaUrlProvider.GetUrl(media));
    }

    [GeneratedRegex("{localLink:(?<udi>umb:.+)}")]
    private static partial Regex LegacyLocalLinkRegex();

    [GeneratedRegex("{localLink:(?<guid>.+)}")]
    private static partial Regex LocalLinkRegex();

    private enum ReplaceStatus
    {
        NoMatch,
        Success,
        InvalidEntityType
    }
}

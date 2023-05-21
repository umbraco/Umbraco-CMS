using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Web.BackOffice.PropertyEditors;

/// <summary>
///     A controller used for the embed dialog
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class RteEmbedController : UmbracoAuthorizedJsonController
{
    private readonly EmbedProvidersCollection _embedCollection;
    private readonly ILogger<RteEmbedController> _logger;

    public RteEmbedController(EmbedProvidersCollection embedCollection, ILogger<RteEmbedController> logger)
    {
        _embedCollection = embedCollection ?? throw new ArgumentNullException(nameof(embedCollection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public OEmbedResult GetEmbed(string url, int width, int height)
    {
        var result = new OEmbedResult();
        var foundMatch = false;
        IEmbedProvider? matchedProvider = null;

        foreach (IEmbedProvider provider in _embedCollection)
        {
            // UrlSchemeRegex is an array of possible regex patterns to match against the URL
            foreach (var urlPattern in provider.UrlSchemeRegex)
            {
                var regexPattern = new Regex(urlPattern, RegexOptions.IgnoreCase);
                if (regexPattern.IsMatch(url))
                {
                    foundMatch = true;
                    matchedProvider = provider;
                    break;
                }
            }

            if (foundMatch)
            {
                break;
            }
        }

        if (foundMatch == false)
        {
            //No matches return/ exit
            result.OEmbedStatus = OEmbedStatus.NotSupported;
            return result;
        }

        try
        {
            result.SupportsDimensions = true;
            result.Markup = matchedProvider?.GetMarkup(url, width, height);
            result.OEmbedStatus = OEmbedStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error embedding URL {Url} - width: {Width} height: {Height}", url, width, height);
            result.OEmbedStatus = OEmbedStatus.Error;
        }

        return result;
    }
}

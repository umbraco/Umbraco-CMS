using System;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Core.Media;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Media.EmbedProviders;

namespace Umbraco.Web.BackOffice.PropertyEditors
{
    /// <summary>
    /// A controller used for the embed dialog
    /// </summary>
    [PluginController("UmbracoApi")]
    public class RteEmbedController : UmbracoAuthorizedJsonController
    {
        private readonly EmbedProvidersCollection _embedCollection;
        private readonly ILogger _logger;

        public RteEmbedController(EmbedProvidersCollection embedCollection, ILogger logger)
        {
            _embedCollection = embedCollection ?? throw new ArgumentNullException(nameof(embedCollection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public OEmbedResult GetEmbed(string url, int width, int height)
        {
            var result = new OEmbedResult();
            var foundMatch = false;
            IEmbedProvider matchedProvider = null;

            foreach (var provider in _embedCollection)
            {
                //Url Scheme Regex is an array of possible regex patterns to match against the URL
                foreach(var urlPattern in provider.UrlSchemeRegex)
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
                    break;
            }

            if(foundMatch == false)
            {
                //No matches return/ exit
                result.OEmbedStatus = OEmbedStatus.NotSupported;
                return result;
            }

            try
            {
                result.SupportsDimensions = true;
                result.Markup = matchedProvider.GetMarkup(url, width, height);
                result.OEmbedStatus = OEmbedStatus.Success;
            }
            catch(Exception ex)
            {
                _logger.Error<RteEmbedController>(ex, "Error embedding url {Url} - width: {Width} height: {Height}", url, width, height);
                result.OEmbedStatus = OEmbedStatus.Error;
            }

            return result;
        }
    }
}

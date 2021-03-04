using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Web
{
    public class AspNetBackOfficeInfo : IBackOfficeInfo
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IIOHelper _ioHelper;
        private readonly ILogger<AspNetBackOfficeInfo> _logger;
        private readonly WebRoutingSettings _webRoutingSettings;

        public AspNetBackOfficeInfo(GlobalSettings globalSettings, IIOHelper ioHelper, ILogger<AspNetBackOfficeInfo> logger, IOptions<WebRoutingSettings> webRoutingSettings)
        {
            _globalSettings = globalSettings;
            _ioHelper = ioHelper;
            _logger = logger;
            _webRoutingSettings = webRoutingSettings.Value;
        }

        /// <inheritdoc />
        public string GetAbsoluteUrl => GetAbsoluteUrlFromConfig() ?? GetAbsoluteUrlFromCurrentRequest() ?? null;

        private string GetAbsoluteUrlFromConfig()
        {
            var url = _webRoutingSettings.UmbracoApplicationUrl;
            if (url.IsNullOrWhiteSpace() == false)
            {
                var umbracoApplicationUrl = url.TrimEnd('/');
                _logger.LogInformation("ApplicationUrl: {UmbracoAppUrl} (using web.routing/@umbracoApplicationUrl)", umbracoApplicationUrl);
                return umbracoApplicationUrl;
            }

            return null;
        }



        private string GetAbsoluteUrlFromCurrentRequest()
        {
            var request = HttpContext.Current?.Request;

            if (request is null) return null;

            // if (HTTP and SSL not required) or (HTTPS and SSL required),
            //  use ports from request
            // otherwise,
            //  if non-standard ports used,
            //  user may need to set umbracoApplicationUrl manually per
            //  https://our.umbraco.com/documentation/Using-Umbraco/Config-files/umbracoSettings/#ScheduledTasks
            var port = (request.IsSecureConnection == false && _globalSettings.UseHttps == false)
                       || (request.IsSecureConnection && _globalSettings.UseHttps)
                ? ":" + request.ServerVariables["SERVER_PORT"]
                : "";

            var ssl = _globalSettings.UseHttps ? "s" : ""; // force, whatever the first request
            var url = "http" + ssl + "://" + request.ServerVariables["SERVER_NAME"] + port +
                      _ioHelper.ResolveUrl(_globalSettings.UmbracoPath);

            return url.TrimEnd('/');
        }
    }
}

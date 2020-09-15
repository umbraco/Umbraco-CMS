using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Web
{
    public class AspNetBackOfficeInfo : IBackOfficeInfo
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IIOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly IWebRoutingSettings _webRoutingSettings;

        public AspNetBackOfficeInfo(IGlobalSettings globalSettings, IIOHelper ioHelper, ILogger logger, IWebRoutingSettings webRoutingSettings)
        {
            _globalSettings = globalSettings;
            _ioHelper = ioHelper;
            _logger = logger;
            _webRoutingSettings = webRoutingSettings;
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

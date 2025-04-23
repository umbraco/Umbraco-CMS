using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCoreBackOfficeInfo : IBackOfficeInfo
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private string? _getAbsoluteUrl;

    public AspNetCoreBackOfficeInfo(IHostingEnvironment hostingEnviroment)
        => _hostingEnvironment = hostingEnviroment;

    [Obsolete("The globalSettings parameter is not required anymore, use the other constructor instead. Scheduled for removal in Umbraco 17.")]
    public AspNetCoreBackOfficeInfo(IOptionsMonitor<GlobalSettings> globalSettings, IHostingEnvironment hostingEnviroment)
        : this(hostingEnviroment)
    { }

    public string GetAbsoluteUrl
    {
        get
        {
            if (_getAbsoluteUrl is null)
            {
                if (_hostingEnvironment.ApplicationMainUrl is null)
                {
                    return string.Empty;
                }

                _getAbsoluteUrl = WebPath.Combine(
                    _hostingEnvironment.ApplicationMainUrl.ToString(),
                    Core.Constants.System.DefaultUmbracoPath.TrimStart(Core.Constants.CharArrays.TildeForwardSlash));
            }

            return _getAbsoluteUrl;
        }
    }
}

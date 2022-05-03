using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;
using static Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCoreBackOfficeInfo : IBackOfficeInfo
{
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private string? _getAbsoluteUrl;

    public AspNetCoreBackOfficeInfo(
        IOptionsMonitor<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnviroment)
    {
        _globalSettings = globalSettings;
        _hostingEnvironment = hostingEnviroment;
    }

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
                    _globalSettings.CurrentValue.UmbracoPath.TrimStart(CharArrays.TildeForwardSlash));
            }

            return _getAbsoluteUrl;
        }
    }
}

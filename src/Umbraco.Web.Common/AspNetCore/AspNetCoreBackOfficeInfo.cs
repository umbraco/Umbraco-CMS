using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.AspNetCore
{
    public class AspNetCoreBackOfficeInfo : IBackOfficeInfo
    {
        public AspNetCoreBackOfficeInfo(IOptionsMonitor<GlobalSettings> globalSettings)
        {
            GetAbsoluteUrl = globalSettings.CurrentValue.UmbracoPath;
        }

        public string GetAbsoluteUrl { get; } // TODO make absolute

    }
}

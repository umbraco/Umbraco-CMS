using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Common.AspNetCore
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

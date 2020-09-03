using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreBackOfficeInfo : IBackOfficeInfo
    {
        public AspNetCoreBackOfficeInfo(IOptions<GlobalSettings> globalSettings)
        {
            GetAbsoluteUrl = globalSettings.Value.UmbracoPath;
        }

        public string GetAbsoluteUrl { get; } // TODO make absolute

    }
}

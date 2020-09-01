using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreBackOfficeInfo : IBackOfficeInfo
    {
        public AspNetCoreBackOfficeInfo(IOptions<GlobalSettings> globalSettings)
            : this(globalSettings.Value)
        {
        }

        public AspNetCoreBackOfficeInfo(GlobalSettings globalSettings)
        {
            GetAbsoluteUrl = globalSettings.UmbracoPath;
        }

        public string GetAbsoluteUrl { get; } // TODO make absolute

    }
}

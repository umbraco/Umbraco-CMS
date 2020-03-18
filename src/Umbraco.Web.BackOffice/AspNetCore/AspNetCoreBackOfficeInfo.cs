using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public class AspNetCoreBackOfficeInfo : IBackOfficeInfo
    {
        public AspNetCoreBackOfficeInfo(IGlobalSettings globalSettings)
        {
            GetAbsoluteUrl = globalSettings.UmbracoPath;
        }

        public string GetAbsoluteUrl { get; }

    }
}

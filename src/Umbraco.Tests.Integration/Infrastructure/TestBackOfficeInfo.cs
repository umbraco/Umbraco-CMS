using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Tests.Integration.Infrastructure
{

    public class TestBackOfficeInfo : IBackOfficeInfo
    {
        public TestBackOfficeInfo(IGlobalSettings globalSettings)
        {
            GetAbsoluteUrl = globalSettings.UmbracoPath;
        }

        public string GetAbsoluteUrl { get; }

    }
}

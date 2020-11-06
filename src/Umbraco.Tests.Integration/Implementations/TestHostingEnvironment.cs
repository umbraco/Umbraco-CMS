using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Web.Common.AspNetCore;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Tests.Integration.Implementations
{
    public class TestHostingEnvironment : AspNetCoreHostingEnvironment, IHostingEnvironment
    {
        public TestHostingEnvironment(IOptionsMonitor<HostingSettings> hostingSettings, IWebHostEnvironment webHostEnvironment)
            : base(hostingSettings, webHostEnvironment)
        {
        }

        /// <summary>
        /// Override for tests since we are not hosted
        /// </summary>
        /// <remarks>
        /// This is specifically used by IOHelper and we want this to return false so that the root path is manually calcualted which is what we want for tests.
        /// </remarks>
        bool IHostingEnvironment.IsHosted { get; } = false;
    }
}

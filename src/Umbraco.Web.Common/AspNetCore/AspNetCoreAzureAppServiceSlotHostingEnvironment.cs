using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.AspNetCore
{
    /// <summary>
    /// An HostingEnvironment specifically for Azure App Service Slots that
    /// provides an ApplicationId unique to a slot. This is required for acquiring/maintaining
    /// the MainDom.
    /// </summary>
    public class AspNetCoreAzureAppServiceSlotHostingEnvironment : AspNetCoreHostingEnvironment, IHostingEnvironment
    {
        private const string SlotInstanceIdKey = "WEBSITE_INSTANCE_ID";
        private const string SlotDeploymentIdKey = "WEBSITE_DEPLOYMENT_ID";

        private string _applicationId;

        public AspNetCoreAzureAppServiceSlotHostingEnvironment(
            IServiceProvider serviceProvider,
            IOptionsMonitor<HostingSettings> hostingSettings,
            IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
            IWebHostEnvironment webHostEnvironment)
            : base(serviceProvider, hostingSettings, webRoutingSettings, webHostEnvironment)
        {
        }

        /// <inheritdoc/>
        string IHostingEnvironment.ApplicationId
        {
            get
            {
                if (_applicationId != null)
                {
                    return _applicationId;
                }

                var slotInstanceId = Environment.GetEnvironmentVariable(SlotInstanceIdKey);
                if (string.IsNullOrWhiteSpace(slotInstanceId))
                {
                    throw new InvalidOperationException("Could not find a WEBSITE_INSTANCE_ID environment variable which suggests this is not an Azure AppService Slot.");
                }

                var deploymentId = Environment.GetEnvironmentVariable(SlotDeploymentIdKey);
                if (string.IsNullOrWhiteSpace(deploymentId))
                {
                    throw new InvalidOperationException("Could not find a WEBSITE_DEPLOYMENT_ID environment variable which suggests this is not an Azure AppService Slot.");
                }

                return _applicationId = $"{slotInstanceId}:{deploymentId}";
            }
        }
    }
}

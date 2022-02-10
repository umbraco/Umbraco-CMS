using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.AspNetCore;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Web.Common.Azure
{
    /// <summary>
    /// Azure App Service Environment
    /// </summary>
    public class AzureAppServiceAspNetCoreHostingEnvironment : AspNetCoreHostingEnvironment, IHostingEnvironment
    {
        /// <summary>
        /// The Environment Variable containing the Azure Website Instance (App Service Plan + App Service Slot)
        /// </summary>
        private const string AZUREWEBSITEINSTANCEID = "WEBSITE_INSTANCE_ID";
        private readonly IServiceProvider _serviceProvider;
        private string _applicationId;

        /// <inheritdoc/>
        public AzureAppServiceAspNetCoreHostingEnvironment(IServiceProvider serviceProvider,
            IOptionsMonitor<HostingSettings> hostingSettings,
            IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
            IWebHostEnvironment webHostEnvironment)
            : base(serviceProvider, hostingSettings, webRoutingSettings, webHostEnvironment)
        {
            _serviceProvider = serviceProvider;
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

                var appId = _serviceProvider.GetApplicationUniqueIdentifier();
                if (appId == null)
                {
                    throw new InvalidOperationException("Could not acquire an ApplicationId, ensure DataProtection services and an IHostEnvironment are registered");
                }

                var websiteIntance = Environment.GetEnvironmentVariable(AZUREWEBSITEINSTANCEID);
                if (string.IsNullOrWhiteSpace(websiteIntance))
                {
                    throw new InvalidOperationException($"Could not acquire an WEBSITE_INSTANCE_ID, ensure {nameof(AzureAppServiceAspNetCoreHostingEnvironment)} is only registered as the IHostingEnvironment on Azure App Service");
                }

                var appInstanceId = string.Concat(appId, ":::", websiteIntance);

                // Hash this value because it can really be anything. By default this will be the application's path.
                // TODO: Test on IIS, hopefully this would be equivalent to the IIS unique ID.
                // This could also contain sensitive information (i.e. like the physical path) which we don't want to expose in logs.
                _applicationId = appInstanceId.GenerateHash();

                return _applicationId;
            }
        }

    }
}

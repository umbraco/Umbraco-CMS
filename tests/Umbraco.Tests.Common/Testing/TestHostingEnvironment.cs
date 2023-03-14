// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.AspNetCore;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.Common.Testing;

public class TestHostingEnvironment : AspNetCoreHostingEnvironment, IHostingEnvironment
{
    public TestHostingEnvironment(
        IOptionsMonitor<HostingSettings> hostingSettings,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        IWebHostEnvironment webHostEnvironment)
        : base(hostingSettings, webRoutingSettings, webHostEnvironment)
    {
    }

    // override
    string IHostingEnvironment.ApplicationId { get; } = "TestApplication";


    /// <summary>
    ///     Gets a value indicating whether we are hosted.
    /// </summary>
    /// <remarks>
    ///     This is specifically used by IOHelper and we want this to return false so that the root path is manually
    ///     calculated which is what we want for tests.
    /// </remarks>
    bool IHostingEnvironment.IsHosted { get; } = false;
}

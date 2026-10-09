// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.HealthChecks;
using Umbraco.Cms.Web.Common.Middleware;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Common.ApplicationBuilder;

[TestFixture]
public class UseUmbracoHealthChecksTests
{
    [Test]
    public async Task Ready_WhenLevelIsRun_Returns200()
    {
        using IHost host = await StartHostAsync(RuntimeLevel.Run);

        using HttpResponseMessage response = await host.GetTestClient().GetAsync("/umbraco/api/health/ready");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    // BootFailed is deliberately excluded: in the real pipeline BootFailedMiddleware is registered
    // ahead of the health checks and short-circuits a BootFailed runtime before /ready is reached
    // (see Ready_WhenBootFailed_IsInterceptedAndReturns500). The remaining non-Run levels do reach
    // the readiness check.
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Upgrading)]
    public async Task Ready_WhenLevelIsNotRun_Returns503(RuntimeLevel level)
    {
        using IHost host = await StartHostAsync(level);

        using HttpResponseMessage response = await host.GetTestClient().GetAsync("/umbraco/api/health/ready");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrading)]
    public async Task Live_RegardlessOfLevel_Returns200(RuntimeLevel level)
    {
        using IHost host = await StartHostAsync(level);

        using HttpResponseMessage response = await host.GetTestClient().GetAsync("/umbraco/api/health/live");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Ready_WhenBootFailed_IsInterceptedAndReturns500()
    {
        // Mirrors the production pipeline order in UseUmbracoRouting: BootFailedMiddleware runs
        // before the health checks, so a BootFailed runtime never reaches the readiness check and
        // surfaces as 500 rather than 503.
        using IHost host = await StartHostAsync(RuntimeLevel.BootFailed, withBootFailedMiddleware: true);

        using HttpResponseMessage response = await host.GetTestClient().GetAsync("/umbraco/api/health/ready");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    private static async Task<IHost> StartHostAsync(RuntimeLevel level, bool withBootFailedMiddleware = false)
    {
        IHost host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(Mock.Of<IRuntimeState>(s => s.Level == level));
                        services.AddHealthChecks()
                            .AddCheck<UmbracoReadinessHealthCheck>(
                                "umbraco-ready",
                                tags: [UmbracoReadinessHealthCheck.ReadyTag]);

                        if (withBootFailedMiddleware)
                        {
                            services.AddSingleton(Mock.Of<IHostingEnvironment>(e => e.IsDebugMode == false));
                            services.AddSingleton(Mock.Of<IWebHostEnvironment>(e => e.WebRootFileProvider == new NullFileProvider()));
                            services.AddSingleton<BootFailedMiddleware>();
                        }
                    })
                    .Configure(app =>
                    {
                        if (withBootFailedMiddleware)
                        {
                            app.UseMiddleware<BootFailedMiddleware>();
                        }

                        app.UseUmbracoHealthChecks();
                    });
            })
            .Build();

        await host.StartAsync();
        return host;
    }
}

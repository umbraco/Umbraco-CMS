// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Web.Common.Hosting;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.ApplicationBuilder;

[TestFixture]
public class UseUmbracoBackOfficeCacheHeadersTests
{
    private const string Prefix = "/umbraco/backoffice/hash123";
    private const string ImmutableValue = "public, max-age=31536000, immutable";

    [Test]
    public async Task ProductionMode_AssetUnderPrefix_SetsImmutable()
    {
        using var host = await StartHostAsync(isDebug: false);

        using var response = await host.GetTestClient().GetAsync($"{Prefix}/css/app.css");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(GetCacheControl(response), Is.EqualTo(ImmutableValue));
        });
    }

    [Test]
    public async Task DebugMode_AssetUnderPrefix_SetsNoCache()
    {
        using var host = await StartHostAsync(isDebug: true);

        using var response = await host.GetTestClient().GetAsync($"{Prefix}/css/app.css");

        Assert.That(GetCacheControl(response), Is.EqualTo("no-cache"));
    }

    [Test]
    public async Task NonSuccessResponseUnderPrefix_DoesNotSetHeader()
    {
        using var host = await StartHostAsync(isDebug: false, terminal: ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        using var response = await host.GetTestClient().GetAsync($"{Prefix}/missing.js");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(GetCacheControl(response), Is.Null);
        });
    }

    [Test]
    public async Task PathOutsidePrefix_DoesNotSetHeader()
    {
        using var host = await StartHostAsync(isDebug: false);

        using var response = await host.GetTestClient().GetAsync("/umbraco/management/api/something");

        Assert.That(GetCacheControl(response), Is.Null);
    }

    [Test]
    public async Task ConsumerSyncOverride_TakesPrecedence()
    {
        using var host = await StartHostAsync(isDebug: false, terminal: ctx =>
        {
            ctx.Response.Headers[HeaderNames.CacheControl] = "no-store";
            return Task.CompletedTask;
        });

        using var response = await host.GetTestClient().GetAsync($"{Prefix}/css/app.css");

        Assert.That(GetCacheControl(response), Is.EqualTo("no-store"));
    }

    [Test]
    public async Task ConsumerOnStartingOverride_TakesPrecedence()
    {
        // Consumer middleware registers OnStarting AFTER ours, so LIFO ordering means the
        // consumer callback fires first and sets the value; our ContainsKey guard then skips.
        using var host = await StartHostAsync(isDebug: false, terminal: ctx =>
        {
            ctx.Response.OnStarting(() =>
            {
                ctx.Response.Headers[HeaderNames.CacheControl] = "private, max-age=60";
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        });

        using var response = await host.GetTestClient().GetAsync($"{Prefix}/css/app.css");

        Assert.That(GetCacheControl(response), Is.EqualTo("private, max-age=60"));
    }

    private static async Task<IHost> StartHostAsync(bool isDebug, RequestDelegate? terminal = null)
    {
        IHost host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(Mock.Of<IBackOfficePathGenerator>(g =>
                            g.BackOfficeAssetsPath == Prefix));
                        services.AddSingleton(Mock.Of<IHostingEnvironment>(e =>
                            e.IsDebugMode == isDebug));
                    })
                    .Configure(app =>
                    {
                        app.UseUmbracoBackOfficeCacheHeaders();
                        app.Run(terminal ?? (ctx =>
                        {
                            ctx.Response.StatusCode = StatusCodes.Status200OK;
                            return Task.CompletedTask;
                        }));
                    });
            })
            .Build();

        await host.StartAsync();
        return host;
    }

    // Read the raw header value via NonValidated to avoid the HttpClient typed parser
    // re-serialising Cache-Control directives into a canonical order (e.g. "private, max-age=60"
    // -> "max-age=60, private"). We care that the exact string we wrote is what's on the wire.
    private static string? GetCacheControl(HttpResponseMessage response)
        => response.Headers.NonValidated.TryGetValues(HeaderNames.CacheControl, out var values)
            ? string.Join(", ", values)
            : null;
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Moq;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Routing;

public class TestRouteBuilder : IEndpointRouteBuilder
{
    private readonly ServiceProvider _serviceProvider;

    public TestRouteBuilder()
    {
        var services = new ServiceCollection();

        var diagnosticListener = new DiagnosticListener("UnitTests");
        services.AddSingleton<DiagnosticSource>(diagnosticListener);
        services.AddSingleton(diagnosticListener);

        services.AddLogging();
        services.AddMvc();
        services.AddSingleton<IHostApplicationLifetime>(x =>
            new ApplicationLifetime(x.GetRequiredService<ILogger<ApplicationLifetime>>()));
        services.AddSignalR();
        _serviceProvider = services.BuildServiceProvider();
    }

    public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();

    public IServiceProvider ServiceProvider => _serviceProvider;

    public IApplicationBuilder CreateApplicationBuilder()
    {
        var mock = new Mock<IApplicationBuilder>();
        mock.Setup(x => x.Build()).Returns(httpContext => Task.CompletedTask);
        return mock.Object;
    }
}

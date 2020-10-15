using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common.Routing
{
    public class TestRouteBuilder : IEndpointRouteBuilder
    {
        private readonly ServiceProvider _serviceProvider;

        public TestRouteBuilder()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMvc();
            services.AddSingleton<IHostApplicationLifetime>(x => new ApplicationLifetime(x.GetRequiredService<ILogger<ApplicationLifetime>>()));
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
}

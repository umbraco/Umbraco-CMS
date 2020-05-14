using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;

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
            _serviceProvider = services.BuildServiceProvider();
        }

        public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();

        public IServiceProvider ServiceProvider => _serviceProvider;

        public IApplicationBuilder CreateApplicationBuilder()
        {
            return Mock.Of<IApplicationBuilder>();
        }
    }
}

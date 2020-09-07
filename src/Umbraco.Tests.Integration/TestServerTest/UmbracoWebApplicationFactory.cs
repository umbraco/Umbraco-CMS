using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Tests.Integration.TestServerTest
{

    public class UmbracoWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly Func<IHostBuilder> _createHostBuilder;

        /// <summary>
        /// Constructor to create a new WebApplicationFactory
        /// </summary>
        /// <param name="createHostBuilder">Method to create the IHostBuilder</param>
        public UmbracoWebApplicationFactory(Func<IHostBuilder> createHostBuilder)
        {
            _createHostBuilder = createHostBuilder;
        }

        protected override IHostBuilder CreateHostBuilder() => _createHostBuilder();
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Controllers;

namespace Umbraco.Tests.Integration.TestServer.Controllers
{
    public class BackOfficeAssetsControllerTests: UmbracoWebApplicationFactory
    {
        private LinkGenerator _linkGenerator;

        [OneTimeSetUp]
        public void GivenARequestToTheController()
        {
            _linkGenerator = Services.GetRequiredService<LinkGenerator>();
        }

        [Test]
        public async Task EnsureSuccessStatusCode()
        {
            // Arrange
            var client = CreateClient();
            var url = _linkGenerator.GetUmbracoApiService<BackOfficeAssetsController>(x=>x.GetSupportedLocales());

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.GreaterOrEqual((int)response.StatusCode, 200);
            Assert.Less((int)response.StatusCode, 300);
        }
    }
}

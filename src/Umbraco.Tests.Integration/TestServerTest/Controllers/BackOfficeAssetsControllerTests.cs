// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Web.BackOffice.Controllers;

namespace Umbraco.Tests.Integration.TestServerTest.Controllers
{
    [TestFixture]
    public class BackOfficeAssetsControllerTests : UmbracoTestServerTestBase
    {
        [Test]
        public async Task EnsureSuccessStatusCode()
        {
            // Arrange
            string url = PrepareUrl<BackOfficeAssetsController>(x => x.GetSupportedLocales());

            // Act
            HttpResponseMessage response = await Client.GetAsync(url);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}

using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Tests.Testing;
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
            var url = PrepareUrl<BackOfficeAssetsController>(x=>x.GetSupportedLocales());

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}

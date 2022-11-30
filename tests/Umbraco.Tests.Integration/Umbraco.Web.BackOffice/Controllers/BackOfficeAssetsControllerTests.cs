// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class BackOfficeAssetsControllerTests : UmbracoTestServerTestBase
{
    [Test]
    public async Task EnsureSuccessStatusCode()
    {
        // Arrange
        var url = PrepareApiControllerUrl<BackOfficeAssetsController>(x => x.GetSupportedLocales());

        // Act
        var response = await Client.GetAsync(url);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}

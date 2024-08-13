using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Preview;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Preview;

public class EndPreviewTests : ManagementApiTest<EndPreviewController>
{
    protected override Expression<Func<EndPreviewController, object>> MethodSelector =>
        x => x.End(CancellationToken.None);


    [Test]
    public virtual async Task As_Anonymous_I_Can_End_Preview_Mode()
    {
        var response = await Client.DeleteAsync(Url);

        // Check if the set cookie header is sent
        var doesHeaderExist = response.Headers.TryGetValues("Set-Cookie", out var setCookieValues) &&
                              setCookieValues.Any(value => value.Contains($"{Constants.Web.PreviewCookieName}=; expires"));

        Assert.IsTrue(doesHeaderExist);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,  await response.Content.ReadAsStringAsync());
    }
}

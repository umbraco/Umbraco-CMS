using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Preview;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Preview;

/// <summary>
/// Verifies that the back-office SignalR hubs reject anonymous connections at the negotiate step.
/// </summary>
public class SignalRHubAuthorizationTests : ManagementApiTest<EndPreviewController>
{
    // Only required by the base fixture; the tests below target the hub endpoints directly.
    protected override Expression<Func<EndPreviewController, object>> MethodSelector { get; set; } =
        x => x.End(CancellationToken.None);

    private static IEnumerable<string> HubPaths =>
    [
        $"/{Constants.System.UmbracoPathSegment}/PreviewHub",
        $"/{Constants.System.UmbracoPathSegment}{Constants.Web.BackofficeSignalRHub}",
        $"/{Constants.System.UmbracoPathSegment}{Constants.Web.ServerEventSignalRHub}",
    ];

    [TestCaseSource(nameof(HubPaths))]
    public async Task Anonymous_Negotiate_Is_Unauthorized(string hubPath)
    {
        HttpResponseMessage response = await Client.PostAsync(NegotiateUrl(hubPath), null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [TestCaseSource(nameof(HubPaths))]
    public async Task Authenticated_Negotiate_Succeeds(string hubPath)
    {
        await AuthenticateClientAsync(Client, "admin@umbraco.com", UserPassword, true);

        HttpResponseMessage response = await Client.PostAsync(NegotiateUrl(hubPath), null);

        var body = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);
        Assert.IsTrue(body.Contains("connectionToken"), body);
    }

    private static string NegotiateUrl(string hubPath) => $"{hubPath}/negotiate?negotiateVersion=1";
}

using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class BackOfficeExternalLoginTests : ManagementApiTest<BackOfficeController>
{
    protected override Expression<Func<BackOfficeController, object>> MethodSelector { get; set; } =
        x => x.ExternalLogin("Umbraco.TestExternalProvider", null);

    // A non-local returnUrl must be rejected before any challenge is issued. This also proves the
    // challenge endpoint is reachable anonymously (a blocked endpoint would answer 401, not 400).
    [Test]
    public async Task ExternalLogin_Challenge_Rejects_NonLocal_ReturnUrl()
    {
        const string url =
            "/umbraco/management/api/v1/security/back-office/external-login"
            + "?provider=Umbraco.TestExternalProvider"
            + "&returnUrl=https%3A%2F%2Fevil.example.com%2Fsteal";

        var response = await Client.GetAsync(url);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await response.Content.ReadAsStringAsync());
    }

    [Test]
    public void ExternalLogin_Challenge_Is_Anonymous_Get_At_Expected_Route()
        => AssertAnonymousGet(nameof(BackOfficeController.ExternalLogin), "external-login");

    [Test]
    public void ExternalLoginCallback_Is_Anonymous_Get_At_Expected_Route()
        => AssertAnonymousGet(nameof(BackOfficeController.ExternalLoginCallback), "ExternalLoginCallback");

    private static void AssertAnonymousGet(string actionName, string expectedRouteTemplate)
    {
        MethodInfo? method = typeof(BackOfficeController).GetMethod(actionName);
        Assert.That(method, Is.Not.Null, $"Action {actionName} should exist");

        Assert.That(
            method!.GetCustomAttribute<AllowAnonymousAttribute>(),
            Is.Not.Null,
            $"{actionName} must be [AllowAnonymous] (login happens before a session exists)");

        var httpGet = method.GetCustomAttribute<HttpGetAttribute>();
        Assert.That(httpGet, Is.Not.Null, $"{actionName} must be an [HttpGet]");
        Assert.That(httpGet!.Template, Is.EqualTo(expectedRouteTemplate), $"{actionName} route template");

        Assert.That(
            method.GetCustomAttribute<MapToApiVersionAttribute>(),
            Is.Not.Null,
            $"{actionName} must carry [MapToApiVersion]");
    }
}

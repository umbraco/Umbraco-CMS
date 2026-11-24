// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.Helpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Helpers;

[TestFixture]
public class OAuthOptionsHelperTests
{
    private static readonly Dictionary<string, StringValues> _callbackParams = new()
    {
        ["error"] = "access_denied",
        ["error_description"] = "The user denied access",
        ["error_uri"] = "https://idp.example.com/errors/access_denied",
        ["state"] = "opaque-state",
    };

    [Test]
    public void SetUmbracoRedirectWithFilteredParams_Copies_Error_Params_From_The_Query_String()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Query = new QueryCollection(_callbackParams);

        Dictionary<string, StringValues> redirectQuery = RedirectQuery(httpContext);

        AssertForwardsErrorParamsOnly(redirectQuery);
    }

    [Test]
    public void SetUmbracoRedirectWithFilteredParams_Copies_Error_Params_From_A_Form_Post()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        httpContext.Request.Form = new FormCollection(_callbackParams);

        Dictionary<string, StringValues> redirectQuery = RedirectQuery(httpContext);

        AssertForwardsErrorParamsOnly(redirectQuery);
    }

    [Test]
    public void SetUmbracoRedirectWithFilteredParams_Encodes_Forwarded_Error_Values()
    {
        const string description = "Cancelled by user & admin (#42 + retry?)";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["error"] = "access_denied",
            ["error_description"] = description,
        });

        Dictionary<string, StringValues> redirectQuery = RedirectQuery(httpContext);

        Assert.Multiple(() =>
        {
            Assert.AreEqual("access_denied", redirectQuery.GetValueOrDefault("error").ToString());
            Assert.AreEqual(description, redirectQuery.GetValueOrDefault("error_description").ToString());
        });
    }

    [Test]
    public void SetUmbracoRedirectWithFilteredParams_Redirects_Without_Error_Params_When_The_Form_Cannot_Be_Read()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        httpContext.Features.Set<IFormFeature>(new ThrowingFormFeature(new IOException("Unexpected end of Stream.")));

        Dictionary<string, StringValues> redirectQuery = RedirectQuery(httpContext);

        Assert.Multiple(() =>
        {
            Assert.AreEqual("external-login", redirectQuery["flow"].ToString());
            Assert.IsFalse(redirectQuery.ContainsKey("error"));
            Assert.IsFalse(redirectQuery.ContainsKey("error_description"));
        });
    }

    private static Dictionary<string, StringValues> RedirectQuery(HttpContext httpContext)
    {
        var helper = new OAuthOptionsHelper(Options.Create(new SecuritySettings()));
        var scheme = new AuthenticationScheme("TestProvider", "Test Provider", typeof(IAuthenticationHandler));
        var context = new AccessDeniedContext(httpContext, scheme, new RemoteAuthenticationOptions());

        helper.SetUmbracoRedirectWithFilteredParams(context, "Test Provider", "OnAccessDenied");

        Assert.AreEqual(StatusCodes.Status302Found, httpContext.Response.StatusCode);
        // Resolve the location as a browser would, so a raw '#' starts the fragment.
        var location = new Uri(new Uri("https://localhost"), httpContext.Response.Headers.Location.ToString());
        return QueryHelpers.ParseQuery(location.Query);
    }

    private static void AssertForwardsErrorParamsOnly(Dictionary<string, StringValues> redirectQuery)
        => Assert.Multiple(() =>
        {
            Assert.AreEqual("external-login", redirectQuery["flow"].ToString());
            Assert.AreEqual("Test Provider", redirectQuery["provider"].ToString());
            Assert.AreEqual("OnAccessDenied", redirectQuery["callback-event"].ToString());
            Assert.AreEqual("access_denied", redirectQuery.GetValueOrDefault("error").ToString());
            Assert.AreEqual("The user denied access", redirectQuery.GetValueOrDefault("error_description").ToString());
            Assert.IsFalse(redirectQuery.ContainsKey("error_uri"));
            Assert.IsFalse(redirectQuery.ContainsKey("state"));
        });
}

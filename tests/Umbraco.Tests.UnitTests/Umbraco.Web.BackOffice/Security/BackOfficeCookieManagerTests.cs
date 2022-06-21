// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Security;

[TestFixture]
public class BackOfficeCookieManagerTests
{
    [Test]
    public void ShouldAuthenticateRequest_When_Not_Configured()
    {
        var globalSettings = new GlobalSettings();

        var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Install);
        var mgr = new BackOfficeCookieManager(
            Mock.Of<IUmbracoContextAccessor>(),
            runtime,
            new UmbracoRequestPaths(Options.Create(globalSettings), TestHelper.GetHostingEnvironment()),
            Mock.Of<IBasicAuthService>());

        var result = mgr.ShouldAuthenticateRequest("/umbraco");

        Assert.IsFalse(result);
    }

    [Test]
    public void ShouldAuthenticateRequest_When_Configured()
    {
        var globalSettings = new GlobalSettings();

        var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);
        var mgr = new BackOfficeCookieManager(
            Mock.Of<IUmbracoContextAccessor>(),
            runtime,
            new UmbracoRequestPaths(
                Options.Create(globalSettings),
                Mock.Of<IHostingEnvironment>(x =>
                    x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco")),
            Mock.Of<IBasicAuthService>());

        var result = mgr.ShouldAuthenticateRequest("/umbraco");

        Assert.IsTrue(result);
    }

    [Test]
    public void ShouldAuthenticateRequest_Is_Back_Office()
    {
        var globalSettings = new GlobalSettings();

        var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);

        GenerateAuthPaths(out var remainingTimeoutSecondsPath, out var isAuthPath);

        var mgr = new BackOfficeCookieManager(
            Mock.Of<IUmbracoContextAccessor>(),
            runtime,
            new UmbracoRequestPaths(
                Options.Create(globalSettings),
                Mock.Of<IHostingEnvironment>(x =>
                    x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco" &&
                    x.ToAbsolute(Constants.SystemDirectories.Install) == "/install")),
            Mock.Of<IBasicAuthService>());

        var result = mgr.ShouldAuthenticateRequest(remainingTimeoutSecondsPath);
        Assert.IsTrue(result);

        result = mgr.ShouldAuthenticateRequest(isAuthPath);
        Assert.IsTrue(result);
    }

    [Test]
    public void ShouldAuthenticateRequest_Not_Back_Office()
    {
        var globalSettings = new GlobalSettings();

        var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);

        var mgr = new BackOfficeCookieManager(
            Mock.Of<IUmbracoContextAccessor>(),
            runtime,
            new UmbracoRequestPaths(
                Options.Create(globalSettings),
                Mock.Of<IHostingEnvironment>(x =>
                    x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco" &&
                    x.ToAbsolute(Constants.SystemDirectories.Install) == "/install")),
            Mock.Of<IBasicAuthService>());

        var result = mgr.ShouldAuthenticateRequest("/notbackoffice");
        Assert.IsFalse(result);
        result = mgr.ShouldAuthenticateRequest("/umbraco/api/notbackoffice");
        Assert.IsFalse(result);
        result = mgr.ShouldAuthenticateRequest("/umbraco/surface/notbackoffice");
        Assert.IsFalse(result);
    }

    private void GenerateAuthPaths(out string remainingTimeoutSecondsPath, out string isAuthPath)
    {
        var controllerName = ControllerExtensions.GetControllerName<AuthenticationController>();

        // this path is not a back office request even though it's in the same controller - it's a 'special' endpoint
        var rPath = remainingTimeoutSecondsPath =
            $"/umbraco/{Constants.Web.Mvc.BackOfficePathSegment}/{Constants.Web.Mvc.BackOfficeApiArea}/{controllerName}/{nameof(AuthenticationController.GetRemainingTimeoutSeconds)}"
                .ToLower();

        // this is on the same controller but is considered a back office request
        var aPath = isAuthPath =
            $"/umbraco/{Constants.Web.Mvc.BackOfficePathSegment}/{Constants.Web.Mvc.BackOfficeApiArea}/{controllerName}/{nameof(AuthenticationController.IsAuthenticated)}"
                .ToLower();
    }
}

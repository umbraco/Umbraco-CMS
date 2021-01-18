// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Routing;
using Umbraco.Extensions;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Backoffice.Security
{
    [TestFixture]
    public class BackOfficeCookieManagerTests
    {
        [Test]
        public void ShouldAuthenticateRequest_When_Not_Configured()
        {
            var globalSettings = new GlobalSettings();

            IRuntimeState runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Install);
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                new UmbracoRequestPaths(Options.Create(globalSettings), TestHelper.GetHostingEnvironment()));

            var result = mgr.ShouldAuthenticateRequest("/umbraco");

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_When_Configured()
        {
            var globalSettings = new GlobalSettings();

            IRuntimeState runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                new UmbracoRequestPaths(
                    Options.Create(globalSettings),
                    Mock.Of<IHostingEnvironment>(x => x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco")));

            var result = mgr.ShouldAuthenticateRequest("/umbraco");

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_Is_Back_Office()
        {
            var globalSettings = new GlobalSettings();

            IRuntimeState runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);

            GenerateAuthPaths(out var remainingTimeoutSecondsPath, out var isAuthPath);

            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                new UmbracoRequestPaths(
                    Options.Create(globalSettings),
                    Mock.Of<IHostingEnvironment>(x => x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco" && x.ToAbsolute(Constants.SystemDirectories.Install) == "/install")));

            var result = mgr.ShouldAuthenticateRequest(remainingTimeoutSecondsPath);
            Assert.IsTrue(result);

            result = mgr.ShouldAuthenticateRequest(isAuthPath);
            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_Not_Back_Office()
        {
            var globalSettings = new GlobalSettings();

            IRuntimeState runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);

            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                new UmbracoRequestPaths(
                    Options.Create(globalSettings),
                    Mock.Of<IHostingEnvironment>(x => x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco" && x.ToAbsolute(Constants.SystemDirectories.Install) == "/install")));

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
            var rPath = remainingTimeoutSecondsPath = $"/umbraco/{Constants.Web.Mvc.BackOfficePathSegment}/{Constants.Web.Mvc.BackOfficeApiArea}/{controllerName}/{nameof(AuthenticationController.GetRemainingTimeoutSeconds)}".ToLower();

            // this is on the same controller but is considered a back office request
            var aPath = isAuthPath = $"/umbraco/{Constants.Web.Mvc.BackOfficePathSegment}/{Constants.Web.Mvc.BackOfficeApiArea}/{controllerName}/{nameof(AuthenticationController.IsAuthenticated)}".ToLower();
        }
    }
}

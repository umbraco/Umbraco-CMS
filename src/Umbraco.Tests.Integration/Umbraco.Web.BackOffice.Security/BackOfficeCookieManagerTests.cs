﻿

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Web;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Tests.Security
{
    [TestFixture]
    public class BackOfficeCookieManagerTests 
    {
        [Test]
        public void ShouldAuthenticateRequest_When_Not_Configured()
        {
            var testHelper = new TestHelper();

            var httpContextAccessor = testHelper.GetHttpContextAccessor();
            var globalSettings = new GlobalSettingsBuilder().Build();
            
            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Install);
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                Mock.Of<IHostingEnvironment>(),
                globalSettings,
                Mock.Of<IRequestCache>(),
                Mock.Of<LinkGenerator>());

            var result = mgr.ShouldAuthenticateRequest(new Uri("http://localhost/umbraco"));

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_When_Configured()
        {
            var testHelper = new TestHelper();


            //hostingEnvironment.ToAbsolute(globalSettings.UmbracoPath);

            var httpContextAccessor = testHelper.GetHttpContextAccessor();
            var globalSettings = new GlobalSettingsBuilder().Build();

            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                Mock.Of<IHostingEnvironment>(x => x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco"),
                globalSettings,
                Mock.Of<IRequestCache>(),
                Mock.Of<LinkGenerator>());

            var result = mgr.ShouldAuthenticateRequest(new Uri("http://localhost/umbraco"));

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_Is_Back_Office()
        {
            var testHelper = new TestHelper();

            var httpContextAccessor = testHelper.GetHttpContextAccessor();
            var globalSettings = new GlobalSettingsBuilder().Build();

            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);
            
            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                Mock.Of<IHostingEnvironment>(x => x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco" && x.ToAbsolute(Constants.SystemDirectories.Install) == "/install"),
                globalSettings,
                Mock.Of<IRequestCache>(),
                GetMockLinkGenerator(out var remainingTimeoutSecondsPath, out var isAuthPath));

            var result = mgr.ShouldAuthenticateRequest(new Uri($"http://localhost{remainingTimeoutSecondsPath}"));
            Assert.IsTrue(result);

            result = mgr.ShouldAuthenticateRequest(new Uri($"http://localhost{isAuthPath}"));
            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_Force_Auth()
        {
            var testHelper = new TestHelper();

            var httpContextAccessor = testHelper.GetHttpContextAccessor();
            var globalSettings = new GlobalSettingsBuilder().Build();

            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);

            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                Mock.Of<IHostingEnvironment>(x => x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco" && x.ToAbsolute(Constants.SystemDirectories.Install) == "/install"),
                globalSettings,
                Mock.Of<IRequestCache>(x => x.IsAvailable == true && x.Get(Constants.Security.ForceReAuthFlag) == "not null"),
                GetMockLinkGenerator(out var remainingTimeoutSecondsPath, out var isAuthPath));

            var result = mgr.ShouldAuthenticateRequest(new Uri($"http://localhost/notbackoffice"));
            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldAuthenticateRequest_Not_Back_Office()
        {
            var testHelper = new TestHelper();

            var httpContextAccessor = testHelper.GetHttpContextAccessor();
            var globalSettings = new GlobalSettingsBuilder().Build();

            var runtime = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);

            var mgr = new BackOfficeCookieManager(
                Mock.Of<IUmbracoContextAccessor>(),
                runtime,
                Mock.Of<IHostingEnvironment>(x => x.ApplicationVirtualPath == "/" && x.ToAbsolute(globalSettings.UmbracoPath) == "/umbraco" && x.ToAbsolute(Constants.SystemDirectories.Install) == "/install"),
                globalSettings,
                Mock.Of<IRequestCache>(),
                GetMockLinkGenerator(out var remainingTimeoutSecondsPath, out var isAuthPath));

            var result = mgr.ShouldAuthenticateRequest(new Uri($"http://localhost/notbackoffice"));
            Assert.IsFalse(result);
            result = mgr.ShouldAuthenticateRequest(new Uri($"http://localhost/umbraco/api/notbackoffice"));
            Assert.IsFalse(result);
            result = mgr.ShouldAuthenticateRequest(new Uri($"http://localhost/umbraco/surface/notbackoffice"));
            Assert.IsFalse(result);
        }

        private LinkGenerator GetMockLinkGenerator(out string remainingTimeoutSecondsPath, out string isAuthPath)
        {
            var controllerName = ControllerExtensions.GetControllerName<AuthenticationController>();

            // this path is not a back office request even though it's in the same controller - it's a 'special' endpoint
            var rPath = remainingTimeoutSecondsPath = $"/umbraco/{Constants.Web.Mvc.BackOfficePathSegment}/{Constants.Web.Mvc.BackOfficeApiArea}/{controllerName}/{nameof(AuthenticationController.GetRemainingTimeoutSeconds)}".ToLower();

            // this is on the same controller but is considered a back office request
            var aPath = isAuthPath = $"/umbraco/{Constants.Web.Mvc.BackOfficePathSegment}/{Constants.Web.Mvc.BackOfficeApiArea}/{controllerName}/{nameof(AuthenticationController.IsAuthenticated)}".ToLower();

            var linkGenerator = new Mock<LinkGenerator>();
            linkGenerator.Setup(x => x.GetPathByAddress(
                //It.IsAny<HttpContext>(),
                It.IsAny<RouteValuesAddress>(),
                //It.IsAny<RouteValueDictionary>(),
                It.IsAny<RouteValueDictionary>(),
                It.IsAny<PathString>(),
                It.IsAny<FragmentString>(),
                It.IsAny<LinkOptions>())).Returns((RouteValuesAddress address, RouteValueDictionary routeVals1, PathString path, FragmentString fragment, LinkOptions options) =>
                {
                    if (routeVals1["action"].ToString() == nameof(AuthenticationController.GetRemainingTimeoutSeconds))
                        return rPath;
                    if (routeVals1["action"].ToString() == nameof(AuthenticationController.IsAuthenticated).ToLower())
                        return aPath;
                    return null;
                });

            return linkGenerator.Object;
        }
    }
}

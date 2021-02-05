using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Features;
using Umbraco.Web.Routing;
using Umbraco.Web.Website.Controllers;
using Umbraco.Web.Website.Routing;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Website.Routing
{

    [TestFixture]
    public class UmbracoRouteValuesFactoryTests
    {
        private UmbracoRouteValuesFactory GetFactory(
            out Mock<IPublishedRouter> publishedRouter,
            out UmbracoRenderingDefaults renderingDefaults,
            out IPublishedRequest request)
        {
            var builder = new PublishedRequestBuilder(new Uri("https://example.com"), Mock.Of<IFileService>());
            builder.SetPublishedContent(Mock.Of<IPublishedContent>());
            request = builder.Build();

            publishedRouter = new Mock<IPublishedRouter>();
            publishedRouter.Setup(x => x.UpdateRequestToNotFound(It.IsAny<IPublishedRequest>()))
                .Returns((IPublishedRequest r) => builder)
                .Verifiable();

            renderingDefaults = new UmbracoRenderingDefaults();

            // add the default one
            var actionDescriptors = new List<ActionDescriptor>
            {
                new ControllerActionDescriptor
                {
                    ControllerName = ControllerExtensions.GetControllerName<RenderController>(),
                    ActionName = nameof(RenderController.Index),
                    ControllerTypeInfo = typeof(RenderController).GetTypeInfo()
                }
            };
            var actionSelector = new Mock<IActionSelector>();
            actionSelector.Setup(x => x.SelectCandidates(It.IsAny<RouteContext>())).Returns(actionDescriptors);

            var factory = new UmbracoRouteValuesFactory(
                renderingDefaults,
                Mock.Of<IShortStringHelper>(),
                new UmbracoFeatures(),
                new ControllerActionSearcher(
                    new NullLogger<ControllerActionSearcher>(),
                    actionSelector.Object),
                publishedRouter.Object);

            return factory;
        }

        [Test]
        public void Update_Request_To_Not_Found_When_No_Template()
        {
            UmbracoRouteValuesFactory factory = GetFactory(out Mock<IPublishedRouter> publishedRouter, out _, out IPublishedRequest request);

            UmbracoRouteValues result = factory.Create(new DefaultHttpContext(), request);

            // The request has content, no template, no hijacked route and no disabled template features so UpdateRequestToNotFound will be called
            publishedRouter.Verify(m => m.UpdateRequestToNotFound(It.IsAny<IPublishedRequest>()), Times.Once);
        }

        [Test]
        public void Adds_Result_To_Route_Value_Dictionary()
        {
            UmbracoRouteValuesFactory factory = GetFactory(out _, out UmbracoRenderingDefaults renderingDefaults, out IPublishedRequest request);

            UmbracoRouteValues result = factory.Create(new DefaultHttpContext(), request);

            Assert.IsNotNull(result);
            Assert.AreEqual(renderingDefaults.DefaultControllerType, result.ControllerType);
            Assert.AreEqual(UmbracoRouteValues.DefaultActionName, result.ActionName);
            Assert.IsNull(result.TemplateName);
        }
    }
}

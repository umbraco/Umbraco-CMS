using System;
using Microsoft.AspNetCore.Http;
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
        private UmbracoRouteValuesFactory GetFactory(out Mock<IPublishedRouter> publishedRouter, out UmbracoRenderingDefaults renderingDefaults)
        {
            var builder = new PublishedRequestBuilder(new Uri("https://example.com"), Mock.Of<IFileService>());
            builder.SetPublishedContent(Mock.Of<IPublishedContent>());
            IPublishedRequest request = builder.Build();

            publishedRouter = new Mock<IPublishedRouter>();
            publishedRouter.Setup(x => x.UpdateRequestToNotFound(It.IsAny<IPublishedRequest>()))
                .Returns((IPublishedRequest r) => builder)
                .Verifiable();

            renderingDefaults = new UmbracoRenderingDefaults();

            var factory = new UmbracoRouteValuesFactory(
                renderingDefaults,
                Mock.Of<IShortStringHelper>(),
                new UmbracoFeatures(),
                new HijackedRouteEvaluator(
                    new NullLogger<HijackedRouteEvaluator>(),
                    Mock.Of<IActionDescriptorCollectionProvider>()),
                publishedRouter.Object);

            return factory;
        }

        [Test]
        public void Update_Request_To_Not_Found_When_No_Template()
        {
            var builder = new PublishedRequestBuilder(new Uri("https://example.com"), Mock.Of<IFileService>());
            builder.SetPublishedContent(Mock.Of<IPublishedContent>());
            IPublishedRequest request = builder.Build();

            UmbracoRouteValuesFactory factory = GetFactory(out Mock<IPublishedRouter> publishedRouter, out _);

            UmbracoRouteValues result = factory.Create(new DefaultHttpContext(), new RouteValueDictionary(), request);

            // The request has content, no template, no hijacked route and no disabled template features so UpdateRequestToNotFound will be called
            publishedRouter.Verify(m => m.UpdateRequestToNotFound(It.IsAny<IPublishedRequest>()), Times.Once);
        }

        [Test]
        public void Adds_Result_To_Route_Value_Dictionary()
        {
            var builder = new PublishedRequestBuilder(new Uri("https://example.com"), Mock.Of<IFileService>());
            builder.SetPublishedContent(Mock.Of<IPublishedContent>());
            builder.SetTemplate(Mock.Of<ITemplate>());
            IPublishedRequest request = builder.Build();

            UmbracoRouteValuesFactory factory = GetFactory(out _, out UmbracoRenderingDefaults renderingDefaults);

            var routeVals = new RouteValueDictionary();
            UmbracoRouteValues result = factory.Create(new DefaultHttpContext(), routeVals, request);

            Assert.IsNotNull(result);
            Assert.IsTrue(routeVals.ContainsKey(Constants.Web.UmbracoRouteDefinitionDataToken));
            Assert.AreEqual(result, routeVals[Constants.Web.UmbracoRouteDefinitionDataToken]);
            Assert.AreEqual(renderingDefaults.DefaultControllerType, result.ControllerType);
            Assert.AreEqual(UmbracoRouteValues.DefaultActionName, result.ActionName);
            Assert.IsNull(result.TemplateName);
        }
    }
}

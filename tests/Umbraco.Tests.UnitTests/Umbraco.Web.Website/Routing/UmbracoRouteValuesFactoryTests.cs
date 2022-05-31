using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Routing;

[TestFixture]
public class UmbracoRouteValuesFactoryTests
{
    private UmbracoRouteValuesFactory GetFactory(
        out Mock<IPublishedRouter> publishedRouter,
        out IOptions<UmbracoRenderingDefaultsOptions> renderingDefaults,
        out IPublishedRequest request)
    {
        var builder = new PublishedRequestBuilder(new Uri("https://example.com"), Mock.Of<IFileService>());
        builder.SetPublishedContent(Mock.Of<IPublishedContent>());
        var builtRequest = request = builder.Build();

        publishedRouter = new Mock<IPublishedRouter>();
        publishedRouter.Setup(x => x.UpdateRequestAsync(It.IsAny<IPublishedRequest>(), null))
            .Returns((IPublishedRequest r, IPublishedContent c) => Task.FromResult(builtRequest))
            .Verifiable();

        renderingDefaults =
            Mock.Of<IOptions<UmbracoRenderingDefaultsOptions>>(x =>
                x.Value.DefaultControllerType == typeof(RenderController));

        // add the default one
        var actionDescriptors = new List<ActionDescriptor>
        {
            new ControllerActionDescriptor
            {
                ControllerName = ControllerExtensions.GetControllerName<RenderController>(),
                ActionName = nameof(RenderController.Index),
                ControllerTypeInfo = typeof(RenderController).GetTypeInfo(),
            },
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
    public async Task Update_Request_To_Not_Found_When_No_Template()
    {
        var factory = GetFactory(out var publishedRouter, out _, out var request);

        await factory.CreateAsync(new DefaultHttpContext(), request);

        // The request has content, no template, no hijacked route and no disabled template features so UpdateRequestToNotFound will be called
        publishedRouter.Verify(m => m.UpdateRequestAsync(It.IsAny<IPublishedRequest>(), null), Times.Once);
    }

    [Test]
    public async Task Adds_Result_To_Route_Value_Dictionary()
    {
        var factory = GetFactory(out _, out var renderingDefaults, out var request);

        var result = await factory.CreateAsync(new DefaultHttpContext(), request);

        Assert.IsNotNull(result);
        Assert.AreEqual(renderingDefaults.Value.DefaultControllerType, result.ControllerType);
        Assert.AreEqual(UmbracoRouteValues.DefaultActionName, result.ActionName);
        Assert.IsNull(result.TemplateName);
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.ActionsResults;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.ActionsResults;

[TestFixture]
public class PublishedContentNotFoundResultTests
{
    private const string CustomNotFoundViewPath = "~/Views/CustomNotFound.cshtml";

    [Test]
    public async Task ExecuteResultAsync_RendersTheConfiguredNotFoundView()
    {
        var settings = new GlobalSettings { NotFoundViewPath = CustomNotFoundViewPath };
        var executor = new CapturingViewResultExecutor();

        await ExecuteAsync(settings, executor);

        Assert.That(executor.CapturedViewName, Is.EqualTo(CustomNotFoundViewPath));
    }

    [Test]
    public async Task ExecuteResultAsync_WithoutConfiguration_RendersTheBuiltInNotFoundView()
    {
        var executor = new CapturingViewResultExecutor();

        await ExecuteAsync(new GlobalSettings(), executor);

        Assert.That(executor.CapturedViewName, Is.EqualTo("~/umbraco/UmbracoWebsite/NotFound.cshtml"));
    }

    [Test]
    public async Task ExecuteResultAsync_RespondsWithNotFoundStatusCode()
    {
        var executor = new CapturingViewResultExecutor();

        ActionContext context = await ExecuteAsync(new GlobalSettings(), executor);

        Assert.That(context.HttpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    private static async Task<ActionContext> ExecuteAsync(GlobalSettings settings, IActionResultExecutor<ViewResult> executor)
    {
        IServiceProvider services = new ServiceCollection()
            .AddSingleton<IOptions<GlobalSettings>>(Options.Create(settings))
            .AddSingleton(executor)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        IUmbracoContext umbracoContext = Mock.Of<IUmbracoContext>(c
            => c.OriginalRequestUrl == new Uri("https://example.com/no-such-page"));

        await new PublishedContentNotFoundResult(umbracoContext).ExecuteResultAsync(context);

        return context;
    }

    private sealed class CapturingViewResultExecutor : IActionResultExecutor<ViewResult>
    {
        public string? CapturedViewName { get; private set; }

        public Task ExecuteAsync(ActionContext context, ViewResult result)
        {
            CapturedViewName = result.ViewName;
            return Task.CompletedTask;
        }
    }
}

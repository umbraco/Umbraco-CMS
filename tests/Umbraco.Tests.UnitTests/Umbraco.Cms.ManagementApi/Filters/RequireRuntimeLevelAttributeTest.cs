using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Filters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.ManagementApi.Filters;

[TestFixture]
public class RequireRuntimeLevelAttributeTest
{
    [Test]
    [TestCase(RuntimeLevel.Install, RuntimeLevel.Run, true)]
    [TestCase(RuntimeLevel.Install, RuntimeLevel.Unknown, true)]
    [TestCase(RuntimeLevel.Install, RuntimeLevel.Boot, true)]
    [TestCase(RuntimeLevel.Install, RuntimeLevel.Upgrade, true)]
    [TestCase(RuntimeLevel.Run, RuntimeLevel.Upgrade, true)]
    [TestCase(RuntimeLevel.Install, RuntimeLevel.Install, false)]
    [TestCase(RuntimeLevel.Upgrade, RuntimeLevel.Upgrade, false)]
    public void BlocksWhenIncorrectRuntime(RuntimeLevel requiredLevel, RuntimeLevel actualLevel, bool shouldFail)
    {
        var executionContext = CreateActionExecutingContext(actualLevel);

        var sut = new RequireRuntimeLevelAttribute(requiredLevel);
        sut.OnActionExecuting(executionContext);

        if (shouldFail)
        {
            AssertFailure(executionContext);
            return;
        }

        // Assert success, result being null == we haven't short circuited.
        Assert.IsNull(executionContext.Result);
    }

    private void AssertFailure(ActionExecutingContext executionContext)
    {
        var result = executionContext.Result;
        Assert.IsInstanceOf<ObjectResult>(result);

        var objectResult = (ObjectResult)result;

        Assert.AreEqual(StatusCodes.Status428PreconditionRequired, objectResult?.StatusCode);
        Assert.IsInstanceOf<ProblemDetails>(objectResult?.Value);
    }

    private ActionExecutingContext CreateActionExecutingContext(RuntimeLevel targetRuntimeLevel)
    {
        var actionContext = new ActionContext()
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new());

        var fakeRuntime = new Mock<IRuntimeState>();
        fakeRuntime.Setup(x => x.Level).Returns(targetRuntimeLevel);

        var fakeServiceProvider = new Mock<IServiceProvider>();
        fakeServiceProvider.Setup(x => x.GetService(typeof(IRuntimeState))).Returns(fakeRuntime.Object);
        actionContext.HttpContext.RequestServices = fakeServiceProvider.Object;

        return executingContext;
    }
}

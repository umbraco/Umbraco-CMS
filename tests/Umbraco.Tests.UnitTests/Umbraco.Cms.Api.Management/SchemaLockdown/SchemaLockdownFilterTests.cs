using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaLockdownFilterTests
{
    private static ActionExecutingContext CreateContext(string httpMethod)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = httpMethod;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), controller: null!);
    }

    private static SchemaLockdownFilter CreateFilter(bool enabled)
    {
        var accessor = new SchemaLockdownMatrixAccessor(
            Options.Create(new SchemaLockdownSettings { Enabled = enabled }),
            new SchemaLockdownConfiguratorCollection(() => []));

        return new SchemaLockdownFilter(accessor, Constants.UdiEntityType.DocumentType, SchemaOperation.Create);
    }

    [Test]
    public void Blocks_With_403_When_Locked()
    {
        ActionExecutingContext context = CreateContext(HttpMethods.Post);

        CreateFilter(enabled: true).OnActionExecuting(context);

        var result = context.Result as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
            Assert.That(result.Value, Is.TypeOf<ProblemDetails>());
        });
    }

    [Test]
    public void Does_Not_Block_When_Disabled()
    {
        ActionExecutingContext context = CreateContext(HttpMethods.Post);

        CreateFilter(enabled: false).OnActionExecuting(context);

        Assert.That(context.Result, Is.Null);
    }
}

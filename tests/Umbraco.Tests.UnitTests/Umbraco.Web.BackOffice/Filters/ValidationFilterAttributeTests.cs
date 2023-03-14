// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Web.BackOffice.Filters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Filters;

[TestFixture]
public class ValidationFilterAttributeTests
{
    [Test]
    public void Does_Not_Set_Result_When_No_Errors_In_Model_State()
    {
        // Arrange
        var context = CreateContext();
        var attribute = new ValidationFilterAttribute();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsNull(context.Result);
    }

    [Test]
    public void Returns_Bad_Request_When_Errors_In_Model_State()
    {
        // Arrange
        var context = CreateContext(true);
        var attribute = new ValidationFilterAttribute();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        var typedResult = context.Result as BadRequestObjectResult;
        Assert.IsNotNull(typedResult);
    }

    private static ActionExecutingContext CreateContext(bool withError = false)
    {
        var httpContext = new DefaultHttpContext();

        var modelState = new ModelStateDictionary();
        if (withError)
        {
            modelState.AddModelError(string.Empty, "Error");
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new Mock<Controller>().Object);
    }
}

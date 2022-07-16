// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Filters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Filters;

[TestFixture]
public class AppendUserModifiedHeaderAttributeTests
{
    [Test]
    public void Appends_Header_When_No_User_Parameter_Provider()
    {
        // Arrange
        var context = CreateContext();
        var attribute = new AppendUserModifiedHeaderAttribute();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        context.HttpContext.Response.Headers.TryGetValue("X-Umb-User-Modified", out var headerValue);
        Assert.AreEqual("1", headerValue[0]);
    }

    [Test]
    public void Does_Not_Append_Header_If_Already_Exists()
    {
        // Arrange
        var context = CreateContext("0");
        var attribute = new AppendUserModifiedHeaderAttribute();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        context.HttpContext.Response.Headers.TryGetValue("X-Umb-User-Modified", out var headerValue);
        Assert.AreEqual("0", headerValue[0]);
    }

    [Test]
    public void Does_Not_Append_Header_When_User_Id_Parameter_Provided_And_Does_Not_Match_Current_User()
    {
        // Arrange
        var context = CreateContext(actionArgument: new KeyValuePair<string, object>("UserId", 99));
        var userIdParameter = "UserId";
        var attribute = new AppendUserModifiedHeaderAttribute(userIdParameter);

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsFalse(context.HttpContext.Response.Headers.ContainsKey("X-Umb-User-Modified"));
    }

    [Test]
    public void Appends_Header_When_User_Id_Parameter_Provided_And_Does_Not_Match_Current_User()
    {
        // Arrange
        var context = CreateContext(actionArgument: new KeyValuePair<string, object>("UserId", 100));
        var userIdParameter = "UserId";
        var attribute = new AppendUserModifiedHeaderAttribute(userIdParameter);

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        context.HttpContext.Response.Headers.TryGetValue("X-Umb-User-Modified", out var headerValue);
        Assert.AreEqual("1", headerValue[0]);
    }

    private static ActionExecutingContext CreateContext(
        string headerValue = null,
        KeyValuePair<string, object> actionArgument = default)
    {
        var httpContext = new DefaultHttpContext();
        if (!string.IsNullOrEmpty(headerValue))
        {
            httpContext.Response.Headers.Add("X-Umb-User-Modified", headerValue);
        }

        var currentUserMock = new Mock<IUser>();
        currentUserMock
            .SetupGet(x => x.Id)
            .Returns(100);

        var backofficeSecurityMock = new Mock<IBackOfficeSecurity>();
        backofficeSecurityMock
            .SetupGet(x => x.CurrentUser)
            .Returns(currentUserMock.Object);

        var backofficeSecurityAccessorMock = new Mock<IBackOfficeSecurityAccessor>();
        backofficeSecurityAccessorMock
            .SetupGet(x => x.BackOfficeSecurity)
            .Returns(backofficeSecurityMock.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IBackOfficeSecurityAccessor)))
            .Returns(backofficeSecurityAccessorMock.Object);

        httpContext.RequestServices = serviceProviderMock.Object;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new Mock<Controller>().Object);

        if (!EqualityComparer<KeyValuePair<string, object>>.Default.Equals(actionArgument, default))
        {
            context.ActionArguments.Add(actionArgument);
        }

        return context;
    }
}

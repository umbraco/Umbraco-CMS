// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Web.BackOffice.Filters;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Filters
{
    [TestFixture]
    public class OnlyLocalRequestsAttributeTests
    {
        [Test]
        public void Does_Not_Set_Result_When_No_Remote_Address()
        {
            // Arrange
            ActionExecutingContext context = CreateContext();
            var attribute = new OnlyLocalRequestsAttribute();

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsNull(context.Result);
        }

        [Test]
        public void Does_Not_Set_Result_When_Remote_Address_Is_Null_Ip_Address()
        {
            // Arrange
            ActionExecutingContext context = CreateContext(remoteIpAddress: "::1");
            var attribute = new OnlyLocalRequestsAttribute();

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsNull(context.Result);
        }

        [Test]
        public void Does_Not_Set_Result_When_Remote_Address_Matches_Local_Address()
        {
            // Arrange
            ActionExecutingContext context = CreateContext(remoteIpAddress: "100.1.2.3", localIpAddress: "100.1.2.3");
            var attribute = new OnlyLocalRequestsAttribute();

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsNull(context.Result);
        }

        [Test]
        public void Returns_Not_Found_When_Remote_Address_Does_Not_Match_Local_Address()
        {
            // Arrange
            ActionExecutingContext context = CreateContext(remoteIpAddress: "100.1.2.3", localIpAddress: "100.1.2.2");
            var attribute = new OnlyLocalRequestsAttribute();

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            var typedResult = context.Result as NotFoundResult;
            Assert.IsNotNull(typedResult);
        }

        [Test]
        public void Does_Not_Set_Result_When_Remote_Address_Matches_LoopBack_Address()
        {
            // Arrange
            ActionExecutingContext context = CreateContext(remoteIpAddress: "127.0.0.1", localIpAddress: "::1");
            var attribute = new OnlyLocalRequestsAttribute();

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.IsNull(context.Result);
        }

        [Test]
        public void Returns_Not_Found_When_Remote_Address_Does_Not_Match_LoopBack_Address()
        {
            // Arrange
            ActionExecutingContext context = CreateContext(remoteIpAddress: "100.1.2.3", localIpAddress: "::1");
            var attribute = new OnlyLocalRequestsAttribute();

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            var typedResult = context.Result as NotFoundResult;
            Assert.IsNotNull(typedResult);
        }

        private static ActionExecutingContext CreateContext(string remoteIpAddress = null, string localIpAddress = null)
        {
            var httpContext = new DefaultHttpContext();
            if (!string.IsNullOrEmpty(remoteIpAddress))
            {
                httpContext.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddress);
            }

            if (!string.IsNullOrEmpty(localIpAddress))
            {
                httpContext.Connection.LocalIpAddress = IPAddress.Parse(localIpAddress);
            }

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>().Object);
        }
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security;

[TestFixture]
public class BackOfficeSecurityAccessorTests
{
    [Test]
    public void BackOfficeSecurity_NoOverride_NoHttpContext_ReturnsNull()
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == null);
        var accessor = new BackOfficeSecurityAccessor(httpContextAccessor);

        Assert.That(accessor.BackOfficeSecurity, Is.Null);
    }

    [Test]
    public void BackOfficeSecurity_NoOverride_DelegatesToHttpContext()
    {
        var expected = Mock.Of<IBackOfficeSecurity>();
        var httpContext = new DefaultHttpContext();
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService(typeof(IBackOfficeSecurity))).Returns(expected);
        httpContext.RequestServices = services.Object;

        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        var accessor = new BackOfficeSecurityAccessor(httpContextAccessor);

        Assert.That(accessor.BackOfficeSecurity, Is.SameAs(expected));
    }

    [Test]
    public void Override_SetsAmbientValue()
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == null);
        var accessor = new BackOfficeSecurityAccessor(httpContextAccessor);
        var overrideSecurity = Mock.Of<IBackOfficeSecurity>();

        using var scope = accessor.Override(overrideSecurity);

        Assert.That(accessor.BackOfficeSecurity, Is.SameAs(overrideSecurity));
    }

    [Test]
    public void Override_TakesPrecedenceOverHttpContext()
    {
        var httpContextSecurity = Mock.Of<IBackOfficeSecurity>();
        var httpContext = new DefaultHttpContext();
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService(typeof(IBackOfficeSecurity))).Returns(httpContextSecurity);
        httpContext.RequestServices = services.Object;

        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        var accessor = new BackOfficeSecurityAccessor(httpContextAccessor);

        var overrideSecurity = Mock.Of<IBackOfficeSecurity>();
        using var scope = accessor.Override(overrideSecurity);

        Assert.That(accessor.BackOfficeSecurity, Is.SameAs(overrideSecurity));
    }

    [Test]
    public void Override_Dispose_ClearsAmbientValue()
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == null);
        var accessor = new BackOfficeSecurityAccessor(httpContextAccessor);
        var overrideSecurity = Mock.Of<IBackOfficeSecurity>();

        using (var scope = accessor.Override(overrideSecurity))
        {
            Assert.That(accessor.BackOfficeSecurity, Is.SameAs(overrideSecurity));
        }

        Assert.That(accessor.BackOfficeSecurity, Is.Null);
    }

    [Test]
    public void Override_Dispose_FallsBackToHttpContext()
    {
        var httpContextSecurity = Mock.Of<IBackOfficeSecurity>();
        var httpContext = new DefaultHttpContext();
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService(typeof(IBackOfficeSecurity))).Returns(httpContextSecurity);
        httpContext.RequestServices = services.Object;

        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        var accessor = new BackOfficeSecurityAccessor(httpContextAccessor);

        var overrideSecurity = Mock.Of<IBackOfficeSecurity>();
        using (var scope = accessor.Override(overrideSecurity))
        {
            Assert.That(accessor.BackOfficeSecurity, Is.SameAs(overrideSecurity));
        }

        Assert.That(accessor.BackOfficeSecurity, Is.SameAs(httpContextSecurity));
    }

    [Test]
    public void Override_Nested_InnerDispose_RestoresOuter()
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == null);
        var accessor = new BackOfficeSecurityAccessor(httpContextAccessor);

        var outerSecurity = Mock.Of<IBackOfficeSecurity>();
        var innerSecurity = Mock.Of<IBackOfficeSecurity>();

        using (var outerScope = accessor.Override(outerSecurity))
        {
            Assert.That(accessor.BackOfficeSecurity, Is.SameAs(outerSecurity));

            using (var innerScope = accessor.Override(innerSecurity))
            {
                Assert.That(accessor.BackOfficeSecurity, Is.SameAs(innerSecurity));
            }

            Assert.That(accessor.BackOfficeSecurity, Is.SameAs(outerSecurity));
        }
    }
}

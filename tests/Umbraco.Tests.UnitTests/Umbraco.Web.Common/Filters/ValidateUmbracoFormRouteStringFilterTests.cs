// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.DataProtection;
using NUnit.Framework;
using Umbraco.Cms.Web.Common.Exceptions;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Filters;

[TestFixture]
public class ValidateUmbracoFormRouteStringFilterTests
{
    private IDataProtectionProvider DataProtectionProvider { get; } = new EphemeralDataProtectionProvider();

    [Test]
    public void Validate_Route_String()
    {
        var filter =
            new ValidateUmbracoFormRouteStringAttribute.ValidateUmbracoFormRouteStringFilter(DataProtectionProvider);

        Assert.Throws<HttpUmbracoFormRouteStringException>(() => filter.ValidateRouteString(null, null, null, null));

        const string ControllerName = "Test";
        const string ControllerAction = "Index";
        const string Area = "MyArea";
        var validUfprt =
            EncryptionHelper.CreateEncryptedRouteString(DataProtectionProvider, ControllerName, ControllerAction, Area);

        var invalidUfprt = validUfprt + "z";
        Assert.Throws<HttpUmbracoFormRouteStringException>(() =>
            filter.ValidateRouteString(invalidUfprt, null, null, null));

        Assert.Throws<HttpUmbracoFormRouteStringException>(() =>
            filter.ValidateRouteString(validUfprt, ControllerName, ControllerAction, "doesntMatch"));
        Assert.Throws<HttpUmbracoFormRouteStringException>(() =>
            filter.ValidateRouteString(validUfprt, ControllerName, ControllerAction, null));
        Assert.Throws<HttpUmbracoFormRouteStringException>(() =>
            filter.ValidateRouteString(validUfprt, ControllerName, "doesntMatch", Area));
        Assert.Throws<HttpUmbracoFormRouteStringException>(() =>
            filter.ValidateRouteString(validUfprt, ControllerName, null, Area));
        Assert.Throws<HttpUmbracoFormRouteStringException>(() =>
            filter.ValidateRouteString(validUfprt, "doesntMatch", ControllerAction, Area));
        Assert.Throws<HttpUmbracoFormRouteStringException>(() =>
            filter.ValidateRouteString(validUfprt, null, ControllerAction, Area));

        Assert.DoesNotThrow(() => filter.ValidateRouteString(validUfprt, ControllerName, ControllerAction, Area));
        Assert.DoesNotThrow(() => filter.ValidateRouteString(
            validUfprt,
            ControllerName.ToLowerInvariant(),
            ControllerAction.ToLowerInvariant(),
            Area.ToLowerInvariant()));
    }
}

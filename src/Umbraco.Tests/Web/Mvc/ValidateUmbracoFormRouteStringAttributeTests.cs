using NUnit.Framework;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class ValidateUmbracoFormRouteStringAttributeTests
    {
        [Test]
        public void Validate_Route_String()
        {
            var attribute = new ValidateUmbracoFormRouteStringAttribute();

            Assert.Throws<HttpUmbracoFormRouteStringException>(() => attribute.ValidateRouteString(null, null, null, null));

            const string ControllerName = "Test";
            const string ControllerAction = "Index";
            const string Area = "MyArea";
            var validUfprt = UrlHelperRenderExtensions.CreateEncryptedRouteString(ControllerName, ControllerAction, Area);

            var invalidUfprt = validUfprt + "z";
            Assert.Throws<HttpUmbracoFormRouteStringException>(() => attribute.ValidateRouteString(invalidUfprt, null, null, null));

            Assert.Throws<HttpUmbracoFormRouteStringException>(() => attribute.ValidateRouteString(validUfprt, ControllerName, ControllerAction, "doesntMatch"));
            Assert.Throws<HttpUmbracoFormRouteStringException>(() => attribute.ValidateRouteString(validUfprt, ControllerName, ControllerAction, null));
            Assert.Throws<HttpUmbracoFormRouteStringException>(() => attribute.ValidateRouteString(validUfprt, ControllerName, "doesntMatch", Area));
            Assert.Throws<HttpUmbracoFormRouteStringException>(() => attribute.ValidateRouteString(validUfprt, ControllerName, null, Area));
            Assert.Throws<HttpUmbracoFormRouteStringException>(() => attribute.ValidateRouteString(validUfprt, "doesntMatch", ControllerAction, Area));
            Assert.Throws<HttpUmbracoFormRouteStringException>(() => attribute.ValidateRouteString(validUfprt, null, ControllerAction, Area));

            Assert.DoesNotThrow(() => attribute.ValidateRouteString(validUfprt, ControllerName, ControllerAction, Area));
            Assert.DoesNotThrow(() => attribute.ValidateRouteString(validUfprt, ControllerName.ToLowerInvariant(), ControllerAction.ToLowerInvariant(), Area.ToLowerInvariant()));
        }

    }
}

using System.Collections.Specialized;
using System.Web;
using System.Web.Helpers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Security
{
    [TestFixture]
    public class UmbracoAntiForgeryAdditionalDataProviderTests
    {
        [Test]
        public void Test_Wrapped_Non_BeginUmbracoForm()
        {
            var wrapped = Mock.Of<IAntiForgeryAdditionalDataProvider>(x => x.GetAdditionalData(It.IsAny<HttpContextBase>()) == "custom");
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(wrapped);

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            var data = provider.GetAdditionalData(httpContextFactory.HttpContext);

            Assert.IsTrue(data.DetectIsJson());
            var json = JsonConvert.DeserializeObject<UmbracoAntiForgeryAdditionalDataProvider.AdditionalData>(data);
            Assert.AreEqual(null, json.Ufprt);
            Assert.IsTrue(json.Stamp != default);
            Assert.AreEqual("custom", json.WrappedValue);
        }

        [Test]
        public void Null_Wrapped_Non_BeginUmbracoForm()
        {
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(null);

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            var data = provider.GetAdditionalData(httpContextFactory.HttpContext);

            Assert.IsTrue(data.DetectIsJson());
            var json = JsonConvert.DeserializeObject<UmbracoAntiForgeryAdditionalDataProvider.AdditionalData>(data);
            Assert.AreEqual(null, json.Ufprt);
            Assert.IsTrue(json.Stamp != default);
            Assert.AreEqual("default", json.WrappedValue);
        }

        [Test]
        public void Validate_Non_Json()
        {
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(null);

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            var isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "hello");

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Validate_Invalid_Json()
        {
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(null);

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            var isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '0'}");
            Assert.IsFalse(isValid);

            isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': ''}");
            Assert.IsFalse(isValid);

            isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'hello': 'world'}");
            Assert.IsFalse(isValid);

        }

        [Test]
        public void Validate_No_Request_Ufprt()
        {
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(null);

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            //there is a ufprt in the additional data, but not in the request
            var isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '636970328040070330', 'WrappedValue': 'default', 'Ufprt': 'ASBVDFDFDFDF'}");
            Assert.IsFalse(isValid);
        }

        [Test]
        public void Validate_No_AdditionalData_Ufprt()
        {
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(null);

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            var requestMock = Mock.Get(httpContextFactory.HttpContext.Request);
            requestMock.SetupGet(x => x["ufprt"]).Returns("ABCDEFG");

            //there is a ufprt in the additional data, but not in the request
            var isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '636970328040070330', 'WrappedValue': 'default', 'Ufprt': ''}");
            Assert.IsFalse(isValid);
        }

        [Test]
        public void Validate_No_AdditionalData_Or_Request_Ufprt()
        {
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(null);

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            
            //there is a ufprt in the additional data, but not in the request
            var isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '636970328040070330', 'WrappedValue': 'default', 'Ufprt': ''}");
            Assert.IsTrue(isValid);
        }

        [Test]
        public void Validate_Request_And_AdditionalData_Ufprt()
        {
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(null);

            var routeParams1 = $"{RenderRouteHandler.ReservedAdditionalKeys.Controller}={HttpUtility.UrlEncode("Test")}&{RenderRouteHandler.ReservedAdditionalKeys.Action}={HttpUtility.UrlEncode("Index")}&{RenderRouteHandler.ReservedAdditionalKeys.Area}=Umbraco";
            var routeParams2 = $"{RenderRouteHandler.ReservedAdditionalKeys.Controller}={HttpUtility.UrlEncode("Test")}&{RenderRouteHandler.ReservedAdditionalKeys.Action}={HttpUtility.UrlEncode("Index")}&{RenderRouteHandler.ReservedAdditionalKeys.Area}=Umbraco";

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            var requestMock = Mock.Get(httpContextFactory.HttpContext.Request);
            requestMock.SetupGet(x => x["ufprt"]).Returns(routeParams1.EncryptWithMachineKey());

            var isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '636970328040070330', 'WrappedValue': 'default', 'Ufprt': '" + routeParams2.EncryptWithMachineKey() + "'}");
            Assert.IsTrue(isValid);

            routeParams2 = $"{RenderRouteHandler.ReservedAdditionalKeys.Controller}={HttpUtility.UrlEncode("Invalid")}&{RenderRouteHandler.ReservedAdditionalKeys.Action}={HttpUtility.UrlEncode("Index")}&{RenderRouteHandler.ReservedAdditionalKeys.Area}=Umbraco";
            isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '636970328040070330', 'WrappedValue': 'default', 'Ufprt': '" + routeParams2.EncryptWithMachineKey() + "'}");
            Assert.IsFalse(isValid);
        }

        [Test]
        public void Validate_Wrapped_Request_And_AdditionalData_Ufprt()
        {
            var wrapped = Mock.Of<IAntiForgeryAdditionalDataProvider>(x => x.ValidateAdditionalData(It.IsAny<HttpContextBase>(), "custom") == true);
            var provider = new UmbracoAntiForgeryAdditionalDataProvider(wrapped);

            var routeParams1 = $"{RenderRouteHandler.ReservedAdditionalKeys.Controller}={HttpUtility.UrlEncode("Test")}&{RenderRouteHandler.ReservedAdditionalKeys.Action}={HttpUtility.UrlEncode("Index")}&{RenderRouteHandler.ReservedAdditionalKeys.Area}=Umbraco";
            var routeParams2 = $"{RenderRouteHandler.ReservedAdditionalKeys.Controller}={HttpUtility.UrlEncode("Test")}&{RenderRouteHandler.ReservedAdditionalKeys.Action}={HttpUtility.UrlEncode("Index")}&{RenderRouteHandler.ReservedAdditionalKeys.Area}=Umbraco";

            var httpContextFactory = new FakeHttpContextFactory("/hello/world");
            var requestMock = Mock.Get(httpContextFactory.HttpContext.Request);
            requestMock.SetupGet(x => x["ufprt"]).Returns(routeParams1.EncryptWithMachineKey());

            var isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '636970328040070330', 'WrappedValue': 'default', 'Ufprt': '" + routeParams2.EncryptWithMachineKey() + "'}");
            Assert.IsFalse(isValid);

            isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '636970328040070330', 'WrappedValue': 'custom', 'Ufprt': '" + routeParams2.EncryptWithMachineKey() + "'}");
            Assert.IsTrue(isValid);

            routeParams2 = $"{RenderRouteHandler.ReservedAdditionalKeys.Controller}={HttpUtility.UrlEncode("Invalid")}&{RenderRouteHandler.ReservedAdditionalKeys.Action}={HttpUtility.UrlEncode("Index")}&{RenderRouteHandler.ReservedAdditionalKeys.Area}=Umbraco";
            isValid = provider.ValidateAdditionalData(httpContextFactory.HttpContext, "{'Stamp': '636970328040070330', 'WrappedValue': 'default', 'Ufprt': '" + routeParams2.EncryptWithMachineKey() + "'}");
            Assert.IsFalse(isValid);
        }
    }
}

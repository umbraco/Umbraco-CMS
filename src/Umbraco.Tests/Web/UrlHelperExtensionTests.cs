using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class UrlHelperExtensionTests
    {
        [Test]
        public static void Create_Encrypted_RouteString_From_Anonymous_Object()
        {
            var additionalRouteValues = new
            {
                key1 = "value1",
                key2 = "value2",
                Key3 = "Value3",
                keY4 = "valuE4"
            };

            var encryptedRouteString = UrlHelperRenderExtensions.CreateEncryptedRouteString(
                "FormController",
                "FormAction",
                "",
                additionalRouteValues
            );

            var result = encryptedRouteString.DecryptWithMachineKey();

            const string expectedResult = "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public static void Create_Encrypted_RouteString_From_Dictionary()
        {
            var additionalRouteValues = new Dictionary<string, object>()
            {
                {"key1", "value1"},
                {"key2", "value2"},
                {"Key3", "Value3"},
                {"keY4", "valuE4"}
            };

            var encryptedRouteString = UrlHelperRenderExtensions.CreateEncryptedRouteString(
                "FormController",
                "FormAction",
                "",
                additionalRouteValues
            );

            var result = encryptedRouteString.DecryptWithMachineKey();

            const string expectedResult = "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

            Assert.AreEqual(expectedResult, result);
        }
    }
}

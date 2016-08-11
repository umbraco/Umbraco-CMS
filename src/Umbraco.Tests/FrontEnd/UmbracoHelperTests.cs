using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web;

namespace Umbraco.Tests.FrontEnd
{
    [TestFixture]
    public class UmbracoHelperTests
    {
       
        [Test]
        public void Truncate_Simple()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.Truncate(text, 25).ToString();

            Assert.AreEqual("Hello world, this is some&hellip;", result);
        }

        [Test]
        public void Truncate_Inside_Word()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.Truncate(text, 24).ToString();

            Assert.AreEqual("Hello world, this is som&hellip;", result);
        }

        [Test]
        public void Truncate_With_Tag()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.Truncate(text, 35).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
        }

        [Test]
        public void Create_Encrypted_RouteString_From_Anonymous_Object()
        {
            var additionalRouteValues = new
            {
                key1 = "value1",
                key2 = "value2",
                Key3 = "Value3",
                keY4 = "valuE4"
        };
            var encryptedRouteString = UmbracoHelper.CreateEncryptedRouteString("FormController", "FormAction", "", additionalRouteValues);
            var result = encryptedRouteString.DecryptWithMachineKey();
            var expectedResult = "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Create_Encrypted_RouteString_From_Dictionary()
        {
            var additionalRouteValues = new Dictionary<string, object>()
            {
                {"key1", "value1"},
                {"key2", "value2"},
                {"Key3", "Value3"},
                {"keY4", "valuE4"}
            };
            var encryptedRouteString = UmbracoHelper.CreateEncryptedRouteString("FormController", "FormAction", "", additionalRouteValues);
            var result = encryptedRouteString.DecryptWithMachineKey();
            var expectedResult = "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";
            
            Assert.AreEqual(expectedResult, result);
        }
    }
}

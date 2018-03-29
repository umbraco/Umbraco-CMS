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

        /// <summary>
        /// If a truncated string ends with a space, we should trim the space before appending the ellipsis.
        /// </summary>
        [Test]
        public void Truncate_Simple_With_Trimming()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.Truncate(text, 26).ToString();

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

        [Test]
        public void Truncate_By_Words()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(text, 4).ToString();

            Assert.AreEqual("Hello world, this is&hellip;", result);
        }

        [Test]
        public void Truncate_By_Words_With_Tag()
        {
            var text = "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(text, 4).ToString();

            Assert.AreEqual("Hello world, <b>this</b> is&hellip;", result);
        }

        [Test]
        public void Truncate_By_Words_Mid_Tag()
        {
            var text = "Hello world, this is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(text, 7).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
        }

        [Test]
        public void Strip_All_Html()
        {
            var text = "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.StripHtml(text, null).ToString();

            Assert.AreEqual("Hello world, this is some text with a link", result);
        }

        [Test]
        public void Strip_Specific_Html()
        {
            var text = "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

            string [] tags = {"b"};

            var helper = new UmbracoHelper();

            var result = helper.StripHtml(text, tags).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with a link</a>", result);
        }

        [Test]
        public void Strip_Invalid_Html()
        {
            var text = "Hello world, <bthis</b> is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.StripHtml(text).ToString();

            Assert.AreEqual("Hello world, is some text with a link", result);
        }
    }
}

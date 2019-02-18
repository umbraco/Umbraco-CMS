using System.Web.Mvc;
using NUnit.Framework;
using Umbraco.Web;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class HtmlHelperExtensionMethodsTests
    {
        private const string SampleWithAnchorElement = "Hello world, this is some text <a href='blah'>with a link</a>";
        private const string SampleWithBoldAndAnchorElements = "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

        [SetUp]
        public virtual void Initialize()
        {
            //create an empty htmlHelper
            _htmlHelper = new HtmlHelper(new ViewContext(), new ViewPage());
        }

        private HtmlHelper _htmlHelper;

        [Test]
        public void Wrap_Simple()
        {
            var output = _htmlHelper.Wrap("div", "hello world");
            Assert.AreEqual("<div>hello world</div>", output.ToHtmlString());
        }

        [Test]
        public void Wrap_Object_Attributes()
        {
            var output = _htmlHelper.Wrap("div", "hello world", new {style = "color:red;", onclick = "void();"});
            Assert.AreEqual("<div style=\"color:red;\" onclick=\"void();\">hello world</div>", output.ToHtmlString());
        }

        [Test]
        public static void Truncate_Simple()
        {
            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.Truncate(SampleWithAnchorElement, 25).ToString();

            Assert.AreEqual("Hello world, this is some&hellip;", result);
        }

        [Test]
        public static void When_Truncating_A_String_Ends_With_A_Space_We_Should_Trim_The_Space_Before_Appending_The_Ellipsis()
        {
            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.Truncate(SampleWithAnchorElement, 26).ToString();

            Assert.AreEqual("Hello world, this is some&hellip;", result);
        }

        [Test]
        public static void Truncate_Inside_Word()
        {
            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.Truncate(SampleWithAnchorElement, 24).ToString();

            Assert.AreEqual("Hello world, this is som&hellip;", result);
        }

        [Test]
        public static void Truncate_With_Tag()
        {
            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.Truncate(SampleWithAnchorElement, 35).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
        }

        [Test]
        public static void Truncate_By_Words()
        {
            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.TruncateByWords(SampleWithAnchorElement, 4).ToString();

            Assert.AreEqual("Hello world, this is&hellip;", result);
        }

        [Test]
        public static void Truncate_By_Words_With_Tag()
        {
            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.TruncateByWords(SampleWithBoldAndAnchorElements, 4).ToString();

            Assert.AreEqual("Hello world, <b>this</b> is&hellip;", result);
        }

        [Test]
        public static void Truncate_By_Words_Mid_Tag()
        {
            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.TruncateByWords(SampleWithAnchorElement, 7).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
        }

        [Test]
        public static void Strip_All_Html()
        {
            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.StripHtml(SampleWithBoldAndAnchorElements, null).ToString();

            Assert.AreEqual("Hello world, this is some text with a link", result);
        }

        [Test]
        public static void Strip_Specific_Html()
        {
            string[] tags = { "b" };

            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.StripHtml(SampleWithBoldAndAnchorElements, tags).ToString();

            Assert.AreEqual(SampleWithAnchorElement, result);
        }

        [Test]
        public static void Strip_Invalid_Html()
        {
            const string text = "Hello world, <bthis</b> is some text <a href='blah'>with a link</a>";

            var helper = new HtmlHelper(new ViewContext(), new ViewPage());
            var result = helper.StripHtml(text).ToString();

            Assert.AreEqual("Hello world, is some text with a link", result);
        }
    }
}

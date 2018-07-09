using NUnit.Framework;
using Umbraco.Web;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class HtmlStringUtilitiesTests
    {
        private HtmlStringUtilities _htmlStringUtilities;

        [SetUp]
        public virtual void Initialize()
        {
            
            _htmlStringUtilities = new HtmlStringUtilities();
        }

        [Test]
        public void ReplaceLineBreaksWithHtmlBreak()
        {
            var output = _htmlStringUtilities.ReplaceLineBreaksForHtml("<div><h1>hello world</h1><p>hello world\r\nhello world\rhello world\nhello world</p></div>");
            var expected = "<div><h1>hello world</h1><p>hello world<br />hello world<br />hello world<br />hello world</p></div>";
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void TruncateWithElipsis()
        {
            var output = _htmlStringUtilities.Truncate("hello world", 5, true, false).ToString();
            var expected = "hello&hellip;";
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void TruncateWithoutElipsis()
        {
            var output = _htmlStringUtilities.Truncate("hello world", 5, false, false).ToString();
            var expected = "hello";
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void TruncateShorterWordThanHellip()
        {
            //http://issues.umbraco.org/issue/U4-10478
            var output = _htmlStringUtilities.Truncate("hi", 5, true, false).ToString();
            var expected = "hi";
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void TruncateAndRemoveSpaceBetweenHellipAndWord()
        {
            var output = _htmlStringUtilities.Truncate("hello world", 6 /* hello plus space */, true, false).ToString();
            var expected = "hello&hellip;";
            Assert.AreEqual(expected, output);
        }


    }
}
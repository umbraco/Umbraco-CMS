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
    }
}
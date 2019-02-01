using System.Web.Mvc;
using NUnit.Framework;
using Umbraco.Web;

namespace Umbraco.Tests.Web.Mvc
{
	[TestFixture]
	public class HtmlHelperExtensionMethodsTests
	{
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
        public void GetRelatedLinkHtml_Simple()
        {
            var relatedLink = new Umbraco.Web.Models.RelatedLink {
                Caption = "Link Caption",
                NewWindow = true,
                Link = "https://www.google.com/"
            };
            var output = _htmlHelper.GetRelatedLinkHtml(relatedLink);
            Assert.AreEqual("<a href=\"https://www.google.com/\" target=\"_blank\">Link Caption</a>", output.ToHtmlString());
        }

        [Test]
        public void GetRelatedLinkHtml_HtmlAttributes()
        {
            var relatedLink = new Umbraco.Web.Models.RelatedLink
            {
                Caption = "Link Caption",
                NewWindow = true,
                Link = "https://www.google.com/"
            };
            var output = _htmlHelper.GetRelatedLinkHtml(relatedLink, new { @class = "test-class"});
            Assert.AreEqual("<a class=\"test-class\" href=\"https://www.google.com/\" target=\"_blank\">Link Caption</a>", output.ToHtmlString());
        }
    }
}

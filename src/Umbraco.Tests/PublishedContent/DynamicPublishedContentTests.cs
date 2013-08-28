using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using File = Umbraco.Core.Models.File;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
	public class DynamicPublishedContentTests : DynamicDocumentTestsBase<DynamicPublishedContent, DynamicPublishedContentList>
	{
		internal DynamicPublishedContent GetNode(int id)
		{
			//var template = Template.MakeNew("test", new User(0));
			//var ctx = GetUmbracoContext("/test", template.Id);
			var ctx = GetUmbracoContext("/test", 1234);
			var doc = ctx.ContentCache.GetById(id);
			Assert.IsNotNull(doc);
			var dynamicNode = new DynamicPublishedContent(doc);
			Assert.IsNotNull(dynamicNode);
			return dynamicNode;
		}

		protected override dynamic GetDynamicNode(int id)
		{
			return GetNode(id).AsDynamic();
		}

		[Test]
		public void Custom_Extension_Methods()
		{
			var asDynamic = GetDynamicNode(1173);

			Assert.AreEqual("Hello world", asDynamic.DynamicDocumentNoParameters());
			Assert.AreEqual("Hello world!", asDynamic.DynamicDocumentCustomString("Hello world!"));
			Assert.AreEqual("Hello world!" + 123 + false, asDynamic.DynamicDocumentMultiParam("Hello world!", 123, false));
			Assert.AreEqual("Hello world!" + 123 + false, asDynamic.Children.DynamicDocumentListMultiParam("Hello world!", 123, false));
			Assert.AreEqual("Hello world!" + 123 + false, asDynamic.Children.DynamicDocumentEnumerableMultiParam("Hello world!", 123, false));
			
		}

		[Test]
		public void Returns_IDocument_Object()
		{
			var helper = new TestHelper(GetNode(1173));
			var doc = helper.GetDoc();
			//HasProperty is only a prop on DynamicPublishedContent, NOT IPublishedContent
			Assert.IsFalse(doc.GetType().GetProperties().Any(x => x.Name == "HasProperty"));
		}

		[Test]
		public void Returns_DynamicDocument_Object()
		{
			var helper = new TestHelper(GetNode(1173));
			var doc = helper.GetDocAsDynamic();
			//HasProperty is only a prop on DynamicPublishedContent, NOT IPublishedContent
			Assert.IsTrue(doc.HasProperty(Constants.Conventions.Content.UrlAlias));
		}

		[Test]
		public void Returns_DynamicDocument_Object_After_Casting()
		{
			var helper = new TestHelper(GetNode(1173));
			var doc = helper.GetDoc();
			var ddoc = (dynamic) doc;
			//HasProperty is only a prop on DynamicPublishedContent, NOT IPublishedContent
			Assert.IsTrue(ddoc.HasProperty(Constants.Conventions.Content.UrlAlias));
		}

		/// <summary>
		/// Test class to mimic UmbracoHelper when returning docs
		/// </summary>
		public class TestHelper
		{
			private readonly DynamicPublishedContent _doc;

			public TestHelper(DynamicPublishedContent doc)
			{
				_doc = doc;
			}

			public IPublishedContent GetDoc()
			{
				return _doc;
			}

			public dynamic GetDocAsDynamic()
			{
				return _doc.AsDynamic();
			}
		}
	}
}
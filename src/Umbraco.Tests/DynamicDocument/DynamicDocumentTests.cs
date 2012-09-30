using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.Routing;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Tests.DynamicDocument
{
	[TestFixture]
	public class DynamicDocumentTests : DynamicDocumentTestsBase<Umbraco.Core.Dynamics.DynamicDocument, DynamicDocumentList>
	{
		public override void Initialize()
		{
			base.Initialize();

			PropertyEditorValueConvertersResolver.Current = new PropertyEditorValueConvertersResolver(
				new[]
					{
						typeof(DatePickerPropertyEditorValueConverter),
						typeof(TinyMcePropertyEditorValueConverter),
						typeof(YesNoPropertyEditorValueConverter)
					});
		}

		public override void TearDown()
		{
			base.TearDown();

			PropertyEditorValueConvertersResolver.Reset();
		}

		internal Core.Dynamics.DynamicDocument GetNode(int id)
		{
			//var template = Template.MakeNew("test", new User(0));
			//var ctx = GetUmbracoContext("/test", template.Id);
			var ctx = GetUmbracoContext("/test", 1234);
			var contentStore = new DefaultPublishedContentStore();
			var doc = contentStore.GetDocumentById(ctx, id);
			Assert.IsNotNull(doc);
			var dynamicNode = new Core.Dynamics.DynamicDocument(doc);
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
			//HasProperty is only a prop on DynamicDocument, NOT IDocument
			Assert.IsFalse(doc.GetType().GetProperties().Any(x => x.Name == "HasProperty"));
		}

		[Test]
		public void Returns_DynamicDocument_Object()
		{
			var helper = new TestHelper(GetNode(1173));
			var doc = helper.GetDocAsDynamic();
			//HasProperty is only a prop on DynamicDocument, NOT IDocument
			Assert.IsTrue(doc.HasProperty("umbracoUrlAlias"));
		}

		[Test]
		public void Returns_DynamicDocument_Object_After_Casting()
		{
			var helper = new TestHelper(GetNode(1173));
			var doc = helper.GetDoc();
			var ddoc = (dynamic) doc;
			//HasProperty is only a prop on DynamicDocument, NOT IDocument
			Assert.IsTrue(ddoc.HasProperty("umbracoUrlAlias"));
		}

		/// <summary>
		/// Test class to mimic UmbracoHelper when returning docs
		/// </summary>
		public class TestHelper
		{
			private readonly Core.Dynamics.DynamicDocument _doc;

			public TestHelper(Core.Dynamics.DynamicDocument doc)
			{
				_doc = doc;
			}

			public IDocument GetDoc()
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
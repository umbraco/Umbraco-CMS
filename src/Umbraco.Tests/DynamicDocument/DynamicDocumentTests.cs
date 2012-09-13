using System;
using NUnit.Framework;
using Umbraco.Core.Dynamics;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

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

		

		protected override dynamic GetDynamicNode(int id)
		{
			//var template = Template.MakeNew("test", new User(0));
			//var ctx = GetUmbracoContext("/test", template.Id);
			var ctx = GetUmbracoContext("/test", 1234);
			var contentStore = new DefaultPublishedContentStore();
			var doc = contentStore.GetDocumentById(ctx, id);
			Assert.IsNotNull(doc);
			var dynamicNode = new Core.Dynamics.DynamicDocument(doc);
			Assert.IsNotNull(dynamicNode);
			return dynamicNode.AsDynamic();
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
	}
}
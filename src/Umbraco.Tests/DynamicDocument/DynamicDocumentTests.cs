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

			DynamicDocumentDataSourceResolver.Current = new DynamicDocumentDataSourceResolver(
				new TestDynamicDocumentDataSource());

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

			DynamicDocumentDataSourceResolver.Reset();
			PropertyEditorValueConvertersResolver.Reset();
		}

		private class TestDynamicDocumentDataSource : IDynamicDocumentDataSource
		{
			public Guid GetDataType(string docTypeAlias, string propertyAlias)
			{
				if (propertyAlias == "content")
				{
					//return the rte type id
					return Guid.Parse("5e9b75ae-face-41c8-b47e-5f4b0fd82f83");
				}


				return Guid.Empty;
			}
		}

		protected override dynamic GetDynamicNode(int id)
		{
			var template = Template.MakeNew("test", new User(0));
			var ctx = GetUmbracoContext("/test", template.Id);
			var contentStore = new XmlPublishedContentStore();
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
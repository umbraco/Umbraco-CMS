using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.Routing;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Tests.DynamicDocument
{

	/// <summary>
	/// Unit tests for IDocument and extensions
	/// </summary>
	[TestFixture]
	public class DocumentTests : BaseRoutingTest
	{
		public override void Initialize()
		{
			base.Initialize();
			//need to specify a different callback for testing
			DocumentExtensions.GetPropertyAliasesAndNames = s =>
			{
				var userFields = new Dictionary<string, string>()
						{
							{"property1", "Property 1"},
							{"property2", "Property 2"}					
						};
				if (s == "Child")
				{
					userFields.Add("property4", "Property 4");
				}
				else
				{
					userFields.Add("property3", "Property 3");
				}

				//ensure the standard fields are there
				var allFields = new Dictionary<string, string>()
							{
								{"Id", "Id"},
								{"NodeName", "NodeName"},
								{"NodeTypeAlias", "NodeTypeAlias"},
								{"CreateDate", "CreateDate"},
								{"UpdateDate", "UpdateDate"},
								{"CreatorName", "CreatorName"},
								{"WriterName", "WriterName"},
								{"Url", "Url"}
							};
				foreach (var f in userFields.Where(f => !allFields.ContainsKey(f.Key)))
				{
					allFields.Add(f.Key, f.Value);
				}
				return allFields;
			};
			var routingContext = GetRoutingContext("/test");

			//set the UmbracoContext.Current since the extension methods rely on it
			UmbracoContext.Current = routingContext.UmbracoContext;
		}

		public override void TearDown()
		{
			base.TearDown();
			DocumentExtensions.GetPropertyAliasesAndNames = null;
			UmbracoContext.Current = null;
		}		

		[Test]
		public void To_DataTable()
		{	
			var doc = GetDocument(true, 1);
			var dt = doc.ChildrenAsTable();

			Assert.AreEqual(11, dt.Columns.Count);
			Assert.AreEqual(3, dt.Rows.Count);
			Assert.AreEqual("value4", dt.Rows[0]["Property 1"]);
			Assert.AreEqual("value5", dt.Rows[0]["Property 2"]);
			Assert.AreEqual("value6", dt.Rows[0]["Property 4"]);
			Assert.AreEqual("value7", dt.Rows[1]["Property 1"]);
			Assert.AreEqual("value8", dt.Rows[1]["Property 2"]);
			Assert.AreEqual("value9", dt.Rows[1]["Property 4"]);
			Assert.AreEqual("value10", dt.Rows[2]["Property 1"]);
			Assert.AreEqual("value11", dt.Rows[2]["Property 2"]);
			Assert.AreEqual("value12", dt.Rows[2]["Property 4"]);
		}

		[Test]
		public void To_DataTable_With_Filter()
		{
			var doc = GetDocument(true, 1);
			//change a doc type alias
			((TestDocument) doc.Children.ElementAt(0)).DocumentTypeAlias = "DontMatch";

			var dt = doc.ChildrenAsTable("Child");

			Assert.AreEqual(11, dt.Columns.Count);
			Assert.AreEqual(2, dt.Rows.Count);
			Assert.AreEqual("value7", dt.Rows[0]["Property 1"]);
			Assert.AreEqual("value8", dt.Rows[0]["Property 2"]);
			Assert.AreEqual("value9", dt.Rows[0]["Property 4"]);
			Assert.AreEqual("value10", dt.Rows[1]["Property 1"]);
			Assert.AreEqual("value11", dt.Rows[1]["Property 2"]);
			Assert.AreEqual("value12", dt.Rows[1]["Property 4"]);
		}

		[Test]
		public void To_DataTable_No_Rows()
		{
			var doc = GetDocument(false, 1);			
			var dt = doc.ChildrenAsTable();
			//will return an empty data table
			Assert.AreEqual(0, dt.Columns.Count);
			Assert.AreEqual(0, dt.Rows.Count);			
		}

		private IDocument GetDocument(bool createChildren, int indexVals)
		{
			var d = new TestDocument
				{
					CreateDate = DateTime.Now,
					CreatorId = 1,
					CreatorName = "Shannon",
					DocumentTypeAlias = createChildren? "Parent" : "Child",
					DocumentTypeId = 2,
					Id = 3,
					SortOrder = 4,
					TemplateId = 5,
					UpdateDate = DateTime.Now,
					Path = "-1,3",
					UrlName = "home-page",
					Name = "Page" + Guid.NewGuid().ToString(),
					Version = Guid.NewGuid(),
					WriterId = 1,
					WriterName = "Shannon",
					Parent = null,
					Level = 1,
					Properties = new Collection<IDocumentProperty>(
						new List<IDocumentProperty>()
							{
								new PropertyResult("property1", "value" + indexVals, Guid.NewGuid(), PropertyResultType.UserProperty),
								new PropertyResult("property2", "value" + (indexVals + 1), Guid.NewGuid(), PropertyResultType.UserProperty)
							}),
					Children = new List<IDocument>()
				};
			if (createChildren)
			{
				d.Children = new List<IDocument>()
					{
						GetDocument(false, indexVals + 3),
						GetDocument(false, indexVals + 6),
						GetDocument(false, indexVals + 9)
					};
			}
			if (!createChildren)
			{
				//create additional columns, used to test the different columns for child nodes
				d.Properties.Add(new PropertyResult("property4", "value" + (indexVals + 2), Guid.NewGuid(), PropertyResultType.UserProperty));
			}
			else
			{
				d.Properties.Add(new PropertyResult("property3", "value" + (indexVals + 2), Guid.NewGuid(), PropertyResultType.UserProperty));
			}
			return d;
		}


		private class TestDocument : IDocument
		{
			public IDocument Parent { get; set; }
			public int Id { get; set; }
			public int TemplateId { get; set; }
			public int SortOrder { get; set; }
			public string Name { get; set; }
			public string UrlName { get; set; }
			public string DocumentTypeAlias { get; set; }
			public int DocumentTypeId { get; set; }
			public string WriterName { get; set; }
			public string CreatorName { get; set; }
			public int WriterId { get; set; }
			public int CreatorId { get; set; }
			public string Path { get; set; }
			public DateTime CreateDate { get; set; }
			public DateTime UpdateDate { get; set; }
			public Guid Version { get; set; }
			public int Level { get; set; }
			public Collection<IDocumentProperty> Properties { get; set; }
			public IEnumerable<IDocument> Children { get; set; }
		}

	}

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
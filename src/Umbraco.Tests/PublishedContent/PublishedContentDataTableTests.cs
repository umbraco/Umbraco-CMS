using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.PublishedContent
{
	/// <summary>
	/// Unit tests for IPublishedContent and extensions
	/// </summary>
	[TestFixture]
	public class PublishedContentDataTableTests : BaseRoutingTest
	{
		public override void Initialize()
		{
			base.Initialize();

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically
            var type = new AutoPublishedContentType(0, "anything", new PublishedPropertyType[] {});
            PublishedContentType.GetPublishedContentTypeCallback = (alias) => type;

            // need to specify a different callback for testing
			PublishedContentExtensions.GetPropertyAliasesAndNames = s =>
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
			Umbraco.Web.PublishedContentExtensions.GetPropertyAliasesAndNames = null;
			UmbracoContext.Current = null;
		}		

		[Test]
		public void To_DataTable()
		{	
			var doc = GetContent(true, 1);
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
			var doc = GetContent(true, 1);
			//change a doc type alias
			((TestPublishedContent) doc.Children.ElementAt(0)).DocumentTypeAlias = "DontMatch";

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
			var doc = GetContent(false, 1);			
			var dt = doc.ChildrenAsTable();
			//will return an empty data table
			Assert.AreEqual(0, dt.Columns.Count);
			Assert.AreEqual(0, dt.Rows.Count);			
		}

		private IPublishedContent GetContent(bool createChildren, int indexVals)
		{
		    var contentTypeAlias = createChildren ? "Parent" : "Child";
			var d = new TestPublishedContent
				{
					CreateDate = DateTime.Now,
					CreatorId = 1,
					CreatorName = "Shannon",
                    DocumentTypeAlias = contentTypeAlias,
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
					Properties = new Collection<IPublishedProperty>(
						new List<IPublishedProperty>()
							{
								new PropertyResult("property1", "value" + indexVals, PropertyResultType.UserProperty),
								new PropertyResult("property2", "value" + (indexVals + 1), PropertyResultType.UserProperty)
							}),
					Children = new List<IPublishedContent>()
				};
			if (createChildren)
			{
				d.Children = new List<IPublishedContent>()
					{
						GetContent(false, indexVals + 3),
						GetContent(false, indexVals + 6),
						GetContent(false, indexVals + 9)
					};
			}
			if (!createChildren)
			{
				//create additional columns, used to test the different columns for child nodes
				d.Properties.Add(new PropertyResult("property4", "value" + (indexVals + 2), PropertyResultType.UserProperty));
			}
			else
			{
				d.Properties.Add(new PropertyResult("property3", "value" + (indexVals + 2), PropertyResultType.UserProperty));
			}
			return d;
		}

        // note - could probably rewrite those tests using SolidPublishedContentCache
        // l8tr...
	    private class TestPublishedContent : IPublishedContent
	    {
	        public string Url { get; set; }
	        public PublishedItemType ItemType { get; set; }

	        IPublishedContent IPublishedContent.Parent
	        {
	            get { return Parent; }
	        }

	        IEnumerable<IPublishedContent> IPublishedContent.Children
	        {
	            get { return Children; }
	        }

	        public IPublishedContent Parent { get; set; }
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
            public bool IsDraft { get; set; }
            public int GetIndex() { throw new NotImplementedException();}
            
            public ICollection<IPublishedProperty> Properties { get; set; }

	        public object this[string propertyAlias]
	        {
                get { return GetProperty(propertyAlias).Value; }
	        }

	        public IEnumerable<IPublishedContent> Children { get; set; }

	        public IPublishedProperty GetProperty(string alias)
	        {
	            return Properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
	        }

	        public IPublishedProperty GetProperty(string alias, bool recurse)
	        {
	            var property = GetProperty(alias);
	            if (recurse == false) return property;

	            IPublishedContent content = this;
	            while (content != null && (property == null || property.HasValue == false))
	            {
	                content = content.Parent;
	                property = content == null ? null : content.GetProperty(alias);
	            }

	            return property;
	        }

	        public IEnumerable<IPublishedContent> ContentSet
	        {
	            get { throw new NotImplementedException(); }
	        }

            public PublishedContentType ContentType 
            {
                get { throw new NotImplementedException(); }
            }
	    }
	}
}
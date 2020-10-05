using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using PublishedContentExtensions = Umbraco.Web.PublishedContentExtensions;

namespace Umbraco.Tests.PublishedContent
{
    /// <summary>
    /// Unit tests for IPublishedContent and extensions
    /// </summary>
    [TestFixture]
    public class PublishedContentDataTableTests : BaseWebTest
    {
        public override void SetUp()
        {
            base.SetUp();

            // need to specify a different callback for testing
            PublishedContentExtensions.GetPropertyAliasesAndNames = (services, alias) =>
                {
                    var userFields = new Dictionary<string, string>()
                        {
                            {"property1", "Property 1"},
                            {"property2", "Property 2"}
                        };
                    if (alias == "Child")
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
            var umbracoContext = GetUmbracoContext("/test");

            //set the UmbracoContext.Current since the extension methods rely on it
            Umbraco.Web.Composing.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;
        }

        public override void TearDown()
        {
            base.TearDown();
            Umbraco.Web.PublishedContentExtensions.GetPropertyAliasesAndNames = null;
        }

        [Test]
        public void To_DataTable()
        {
            var doc = GetContent(true, 1);
            var dt = doc.ChildrenAsTable(Current.Services);

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
            var c = (SolidPublishedContent)doc.Children.ElementAt(0);
            c.ContentType = new PublishedContentType(Guid.NewGuid(), 22, "DontMatch", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing);

            var dt = doc.ChildrenAsTable(Current.Services, "Child");

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
            var dt = doc.ChildrenAsTable(Current.Services);
            //will return an empty data table
            Assert.AreEqual(0, dt.Columns.Count);
            Assert.AreEqual(0, dt.Rows.Count);
        }

        private IPublishedContent GetContent(bool createChildren, int indexVals)
        {
            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(Mock.Of<ILogger>())) { Id = 1 });

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeService);
            var contentTypeAlias = createChildren ? "Parent" : "Child";
            var contentType = new PublishedContentType(Guid.NewGuid(), 22, contentTypeAlias, PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing);
            var d = new SolidPublishedContent(contentType)
                {
                    CreateDate = DateTime.Now,
                    CreatorId = 1,
                    CreatorName = "Shannon",
                    Id = 3,
                    SortOrder = 4,
                    TemplateId = 5,
                    UpdateDate = DateTime.Now,
                    Path = "-1,3",
                    UrlSegment = "home-page",
                    Name = "Page" + Guid.NewGuid(),
                    Version = Guid.NewGuid(),
                    WriterId = 1,
                    WriterName = "Shannon",
                    Parent = null,
                    Level = 1,
                    Children = new List<IPublishedContent>()
                };
            d.Properties = new Collection<IPublishedProperty>(new List<IPublishedProperty>
            {
                new RawValueProperty(factory.CreatePropertyType("property1", 1), d, "value" + indexVals),
                new RawValueProperty(factory.CreatePropertyType("property2", 1), d, "value" + (indexVals + 1))
            });
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
                ((Collection<IPublishedProperty>) d.Properties).Add(
                    new RawValueProperty(factory.CreatePropertyType("property4",1), d, "value" + (indexVals + 2)));
            }
            else
            {
                ((Collection<IPublishedProperty>) d.Properties).Add(
                    new RawValueProperty(factory.CreatePropertyType("property3", 1), d, "value" + (indexVals + 2)));
            }

            return d;
        }
    }
}

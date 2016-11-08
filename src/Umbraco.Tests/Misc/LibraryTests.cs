using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using umbraco;
using Umbraco.Core.DI;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Tests.Misc
{

	/// <summary>
	/// Tests for the legacy library class
	/// </summary>
	[TestFixture]
	public class LibraryTests : BaseWebTest
	{
        public override void SetUp()
        {
            base.SetUp();

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically
            // when they are requested, but we must declare those that we
            // explicitely want to be here...

            var propertyTypes = new[]
                {
                    // AutoPublishedContentType will auto-generate other properties
                    new PublishedPropertyType("content", 0, "?"),
                };
            var type = new AutoPublishedContentType(0, "anything", propertyTypes);
            ContentTypesCache.GetPublishedContentTypeByAlias = (alias) => type;
            Debug.Print("INIT LIB {0}",
                ContentTypesCache.Get(PublishedItemType.Content, "anything")
                    .PropertyTypes.Count());

            var umbracoContext = GetUmbracoContext("/test");
            Umbraco.Web.Current.UmbracoContextAccessor.UmbracoContext = umbracoContext;
		}

	    /// <summary>
	    /// sets up resolvers before resolution is frozen
	    /// </summary>
	    protected override void Compose()
	    {
	        base.Compose();

            // required so we can access property.Value
	        Container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>();
	    }

	    [Test]
	    public void Json_To_Xml_Object()
	    {
	        var json = "{ id: 1, name: 'hello', children: [{id: 2, name: 'child1'}, {id:3, name: 'child2'}]}";
	        var result = library.JsonToXml(json);
            Assert.AreEqual(@"<json>
  <id>1</id>
  <name>hello</name>
  <children>
    <id>2</id>
    <name>child1</name>
  </children>
  <children>
    <id>3</id>
    <name>child2</name>
  </children>
</json>".CrLf(), result.Current.OuterXml.CrLf());
	    }

        [Test]
        public void Json_To_Xml_Array()
        {
            var json = "[{id: 2, name: 'child1'}, {id:3, name: 'child2'}]";
            var result = library.JsonToXml(json);
            Assert.AreEqual(@"<json>
  <arrayitem>
    <id>2</id>
    <name>child1</name>
  </arrayitem>
  <arrayitem>
    <id>3</id>
    <name>child2</name>
  </arrayitem>
</json>".CrLf(), result.Current.OuterXml.CrLf());
        }

        [Test]
        public void Json_To_Xml_Error()
        {
            var json = "{ id: 1, name: 'hello', children: }";
            var result = library.JsonToXml(json);
            Assert.IsTrue(result.Current.OuterXml.StartsWith("<error>"));
        }

	    [Test]
		public void Get_Item_User_Property()
		{
			var val = library.GetItem(1173, "content");
			var legacyVal = LegacyGetItem(1173, "content");
			Assert.AreEqual(legacyVal, val);
			Assert.AreEqual("<div>This is some content</div>", val);
		}

		[Test]
		public void Get_Item_Document_Property()
		{
			//first test a single static val
			var val = library.GetItem(1173, "template");
			var legacyVal = LegacyGetItem(1173, "template");
			Assert.AreEqual(legacyVal, val);
			Assert.AreEqual("1234", val);

			//now test them all to see if they all match legacy
			foreach(var s in new[]{"id","parentID","level","writerID","template","sortOrder","createDate","updateDate","nodeName","writerName","path"})
			{
				val = library.GetItem(1173, s);
				legacyVal = LegacyGetItem(1173, s);
				Assert.AreEqual(legacyVal, val);
			}
		}

		[Test]
		public void Get_Item_Invalid_Property()
		{
			var val = library.GetItem(1173, "dontfindme");
			var legacyVal = LegacyGetItem(1173, "dontfindme");
			Assert.AreEqual(legacyVal, val);
			Assert.AreEqual("", val);
		}

		/// <summary>
		/// The old method, just using this to make sure we're returning the correct exact data as before.
		/// </summary>
		/// <param name="nodeId"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		private string LegacyGetItem(int nodeId, string alias)
		{
            var cache = UmbracoContext.Current.ContentCache as PublishedContentCache;
            if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");
            var umbracoXML = cache.GetXml(UmbracoContext.Current.InPreviewMode);

            string xpath = "./{0}";
			if (umbracoXML.GetElementById(nodeId.ToString()) != null)
				if (
					",id,parentID,level,writerID,template,sortOrder,createDate,updateDate,nodeName,writerName,path,"
						.
						IndexOf("," + alias + ",") > -1)
					return umbracoXML.GetElementById(nodeId.ToString()).Attributes.GetNamedItem(alias).Value;
				else if (
					umbracoXML.GetElementById(nodeId.ToString()).SelectSingleNode(string.Format(xpath, alias)) !=
					null)
					return
						umbracoXML.GetElementById(nodeId.ToString()).SelectSingleNode(string.Format(xpath, alias)).ChildNodes[0].
							Value; //.Value + "*";
				else
					return string.Empty;
			else
				return string.Empty;
		}
	}
}

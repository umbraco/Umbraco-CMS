using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Examine;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Tests.Cache.PublishedCache
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
	[TestFixture]
    public class PublishMediaCacheTests : BaseWebTest
	{
        protected override void FreezeResolution()
        {
            PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver();
            base.FreezeResolution();
        }
        
        [Test]
		public void Get_Root_Docs()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot1", mType, user, -1);
			var mRoot2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot2", mType, user, -1);
			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot1.Id);
			var mChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child2", mType, user, mRoot2.Id);
			
			var ctx = GetUmbracoContext("/test", 1234);
            var cache = new ContextualPublishedMediaCache(new PublishedMediaCache(ctx.Application), ctx);
			var roots = cache.GetAtRoot();
			Assert.AreEqual(2, roots.Count());
			Assert.IsTrue(roots.Select(x => x.Id).ContainsAll(new[] {mRoot1.Id, mRoot2.Id}));

		}

		[Test]
		public void Get_Item_Without_Examine()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot", mType, user, -1);
			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot.Id);
			var publishedMedia = PublishedMediaTests.GetNode(mRoot.Id, GetUmbracoContext("/test", 1234));

			Assert.AreEqual(mRoot.Id, publishedMedia.Id);
			Assert.AreEqual(mRoot.CreateDateTime.ToString("dd/MM/yyyy HH:mm:ss"), publishedMedia.CreateDate.ToString("dd/MM/yyyy HH:mm:ss"));
			Assert.AreEqual(mRoot.User.Id, publishedMedia.CreatorId);
			Assert.AreEqual(mRoot.User.Name, publishedMedia.CreatorName);
			Assert.AreEqual(mRoot.ContentType.Alias, publishedMedia.DocumentTypeAlias);
			Assert.AreEqual(mRoot.ContentType.Id, publishedMedia.DocumentTypeId);
			Assert.AreEqual(mRoot.Level, publishedMedia.Level);
			Assert.AreEqual(mRoot.Text, publishedMedia.Name);
			Assert.AreEqual(mRoot.Path, publishedMedia.Path);
			Assert.AreEqual(mRoot.sortOrder, publishedMedia.SortOrder);
			Assert.IsNull(publishedMedia.Parent);
		}

		[TestCase("id")]
		[TestCase("nodeId")]
		[TestCase("__NodeId")]
		public void DictionaryDocument_Id_Keys(string key)
		{
			var dicDoc = GetDictionaryDocument(idKey: key);
			DoAssert(dicDoc);
		}

		[TestCase("template")]
		[TestCase("templateId")]
		public void DictionaryDocument_Template_Keys(string key)
		{
			var dicDoc = GetDictionaryDocument(templateKey: key);
			DoAssert(dicDoc);
		}

		[TestCase("nodeName")]
		[TestCase("__nodeName")]
		public void DictionaryDocument_NodeName_Keys(string key)
		{
			var dicDoc = GetDictionaryDocument(nodeNameKey: key);
			DoAssert(dicDoc);
		}

		[TestCase("nodeTypeAlias")]
		[TestCase("__NodeTypeAlias")]
		public void DictionaryDocument_NodeTypeAlias_Keys(string key)
		{
			var dicDoc = GetDictionaryDocument(nodeTypeAliasKey: key);
			DoAssert(dicDoc);
		}

		[TestCase("path")]
		[TestCase("__Path")]
		public void DictionaryDocument_Path_Keys(string key)
		{
			var dicDoc = GetDictionaryDocument(pathKey: key);
			DoAssert(dicDoc);
		}

		[Test]
		public void DictionaryDocument_Get_Children()
		{
			var child1 = GetDictionaryDocument(idVal: 222333);
			var child2 = GetDictionaryDocument(idVal: 444555);

			var dicDoc = GetDictionaryDocument(children: new List<IPublishedContent>()
				{
					child1, child2
				});
			
			Assert.AreEqual(2, dicDoc.Children.Count());
			Assert.AreEqual(222333, dicDoc.Children.ElementAt(0).Id);
			Assert.AreEqual(444555, dicDoc.Children.ElementAt(1).Id);
		}

		[Test]
		public void Convert_From_Search_Result()
		{
            var ctx = GetUmbracoContext("/test", 1234);

			var result = new SearchResult()
				{
					Id = 1234,
					Score = 1
				};
			result.Fields.Add("__IndexType", "media");
			result.Fields.Add("__NodeId", "1234");
			result.Fields.Add("__NodeTypeAlias", Constants.Conventions.MediaTypes.Image);
			result.Fields.Add("__Path", "-1,1234");
			result.Fields.Add("__nodeName", "Test");
			result.Fields.Add("id", "1234");
			result.Fields.Add("nodeName", "Test");
			result.Fields.Add("nodeTypeAlias", Constants.Conventions.MediaTypes.Image);
			result.Fields.Add("parentID", "-1");
			result.Fields.Add("path", "-1,1234");
			result.Fields.Add("updateDate", "2012-07-16T10:34:09");
			result.Fields.Add("writerName", "Shannon");

            var store = new PublishedMediaCache(ctx.Application);
			var doc = store.ConvertFromSearchResult(result);

			DoAssert(doc, 1234, 0, 0, "", "Image", 0, "Shannon", "", 0, 0, "-1,1234", default(DateTime), DateTime.Parse("2012-07-16T10:34:09"), 2);
			Assert.AreEqual(null, doc.Parent);
		}

		[Test]
		public void Convert_From_XPath_Navigator()
		{
            var ctx = GetUmbracoContext("/test", 1234);

			var xmlDoc = GetMediaXml();
			var navigator = xmlDoc.SelectSingleNode("/root/Image").CreateNavigator();
            var cache = new PublishedMediaCache(ctx.Application);
			var doc = cache.ConvertFromXPathNavigator(navigator);

			DoAssert(doc, 2000, 0, 2, "image1", "Image", 2044, "Shannon", "Shannon2", 22, 33, "-1,2000", DateTime.Parse("2012-06-12T14:13:17"), DateTime.Parse("2012-07-20T18:50:43"), 1);
			Assert.AreEqual(null, doc.Parent);
			Assert.AreEqual(2, doc.Children.Count());
			Assert.AreEqual(2001, doc.Children.ElementAt(0).Id);
			Assert.AreEqual(2002, doc.Children.ElementAt(1).Id);
		}

		private XmlDocument GetMediaXml()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[ 
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>
<!ELEMENT CustomDocument ANY>
<!ATTLIST CustomDocument id ID #REQUIRED>
]>
<root id=""-1"">
	<Image id=""2000"" parentID=""-1"" level=""1"" writerID=""22"" creatorID=""33"" nodeType=""2044"" template=""0"" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Image1"" urlName=""image1"" writerName=""Shannon"" creatorName=""Shannon2"" path=""-1,2000"" isDoc="""">
		<file><![CDATA[/media/1234/image1.png]]></file>		
		<Image id=""2001"" parentID=""2000"" level=""2"" writerID=""22"" creatorID=""33"" nodeType=""2044"" template=""0"" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Image1"" urlName=""image1"" writerName=""Shannon"" creatorName=""Shannon2"" path=""-1,2000,2001"" isDoc="""">
			<file><![CDATA[/media/1234/image1.png]]></file>		
		</Image>
		<Image id=""2002"" parentID=""2000"" level=""2"" writerID=""22"" creatorID=""33"" nodeType=""2044"" template=""0"" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Image1"" urlName=""image1"" writerName=""Shannon"" creatorName=""Shannon2"" path=""-1,2000,2002"" isDoc="""">
			<file><![CDATA[/media/1234/image1.png]]></file>		
		</Image>
	</Image>
</root>";

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			return xmlDoc;
		}

		private Dictionary<string, string> GetDictionary(			
			int id, 
			int parentId,
			string idKey,
			string templateKey,
			string nodeNameKey,
			string nodeTypeAliasKey,
			string pathKey)
		{
			return new Dictionary<string, string>()
				{
					{idKey, id.ToString()},
					{templateKey, "333"},
					{"sortOrder", "44"},
					{nodeNameKey, "Testing"},
					{"urlName", "testing"},
					{nodeTypeAliasKey, "myType"},
					{"nodeType", "22"},
					{"writerName", "Shannon"},
					{"creatorName", "Shannon2"},
					{"writerID", "33"},
					{"creatorID", "44"},
					{pathKey, "1,2,3,4,5"},
					{"createDate", "2012-01-02"},
					{"updateDate", "2012-01-03"},
					{"level", "3"},
					{"parentID", parentId.ToString()}
				};
		}
		
		private PublishedMediaCache.DictionaryPublishedContent GetDictionaryDocument(
			string idKey = "id",
			string templateKey = "template",
			string nodeNameKey = "nodeName",
			string nodeTypeAliasKey = "nodeTypeAlias",
			string pathKey = "path", 
			int idVal = 1234,
			int parentIdVal = 321,
			IEnumerable<IPublishedContent> children = null)
		{
			if (children == null)
				children = new List<IPublishedContent>();
            var dicDoc = new PublishedMediaCache.DictionaryPublishedContent(
				//the dictionary
				GetDictionary(idVal, parentIdVal, idKey, templateKey, nodeNameKey, nodeTypeAliasKey, pathKey),
				//callback to get the parent
                d => new PublishedMediaCache.DictionaryPublishedContent(
						GetDictionary(parentIdVal, -1, idKey, templateKey, nodeNameKey, nodeTypeAliasKey, pathKey),
					//there is no parent
						a => null,
					//we're not going to test this so ignore
						a => new List<IPublishedContent>(),
						(dd, a) => dd.Properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(a)), 
						false),
				//callback to get the children
				d => children,
				(dd, a) => dd.Properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(a)), 
				false);
			return dicDoc;
		}

		private void DoAssert(
			PublishedMediaCache.DictionaryPublishedContent dicDoc,
			int idVal = 1234,
			int templateIdVal = 333,
			int sortOrderVal = 44,
			string urlNameVal = "testing",
			string nodeTypeAliasVal = "myType",
			int nodeTypeIdVal = 22,
			string writerNameVal = "Shannon",
			string creatorNameVal = "Shannon2",
			int writerIdVal = 33,
			int creatorIdVal = 44,
			string pathVal = "1,2,3,4,5",
			DateTime? createDateVal = null,
			DateTime? updateDateVal = null,
			int levelVal = 3,
			int parentIdVal = 321)
		{
			if (!createDateVal.HasValue)
				createDateVal = DateTime.Parse("2012-01-02");
			if (!updateDateVal.HasValue)
				updateDateVal = DateTime.Parse("2012-01-03");

			DoAssert((IPublishedContent)dicDoc, idVal, templateIdVal, sortOrderVal, urlNameVal, nodeTypeAliasVal, nodeTypeIdVal, writerNameVal, 
				creatorNameVal, writerIdVal, creatorIdVal, pathVal, createDateVal, updateDateVal, levelVal);

			//now validate the parentId that has been parsed, this doesn't exist on the IPublishedContent
			Assert.AreEqual(parentIdVal, dicDoc.ParentId);
		}

		private void DoAssert(
			IPublishedContent doc,
			int idVal = 1234,
			int templateIdVal = 333,
			int sortOrderVal = 44,
			string urlNameVal = "testing",
			string nodeTypeAliasVal = "myType",
			int nodeTypeIdVal = 22,
			string writerNameVal = "Shannon",
			string creatorNameVal = "Shannon2",
			int writerIdVal = 33,
			int creatorIdVal = 44,
			string pathVal = "1,2,3,4,5",
			DateTime? createDateVal = null,
			DateTime? updateDateVal = null,
			int levelVal = 3)
		{
			if (!createDateVal.HasValue)
				createDateVal = DateTime.Parse("2012-01-02");
			if (!updateDateVal.HasValue)
				updateDateVal = DateTime.Parse("2012-01-03");

			Assert.AreEqual(idVal, doc.Id);
			Assert.AreEqual(templateIdVal, doc.TemplateId);
			Assert.AreEqual(sortOrderVal, doc.SortOrder);
			Assert.AreEqual(urlNameVal, doc.UrlName);
			Assert.AreEqual(nodeTypeAliasVal, doc.DocumentTypeAlias);
			Assert.AreEqual(nodeTypeIdVal, doc.DocumentTypeId);
			Assert.AreEqual(writerNameVal, doc.WriterName);
			Assert.AreEqual(creatorNameVal, doc.CreatorName);
			Assert.AreEqual(writerIdVal, doc.WriterId);
			Assert.AreEqual(creatorIdVal, doc.CreatorId);
			Assert.AreEqual(pathVal, doc.Path);
			Assert.AreEqual(createDateVal.Value, doc.CreateDate);
			Assert.AreEqual(updateDateVal.Value, doc.UpdateDate);
			Assert.AreEqual(levelVal, doc.Level);
			
		}

		
	}
}
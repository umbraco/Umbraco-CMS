using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Examine;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Current = Umbraco.Web.Composing.Current;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Tests.LegacyXmlPublishedCache;
using Umbraco.Tests.PublishedContent;
using Umbraco.Web;

namespace Umbraco.Tests.Cache.PublishedCache
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class PublishMediaCacheTests : BaseWebTest
    {
        private Dictionary<string, PublishedContentType> _mediaTypes;

        private IUmbracoContextAccessor _umbracoContextAccessor;
        protected override void Compose()
        {
            base.Compose();

            Composition.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>()
                .Clear()
                .Append<DefaultUrlSegmentProvider>();

            _umbracoContextAccessor = Current.UmbracoContextAccessor;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var type = new AutoPublishedContentType(Guid.NewGuid(), 22, "myType", new PublishedPropertyType[] { });
            var image = new AutoPublishedContentType(Guid.NewGuid(), 23, "Image", new PublishedPropertyType[] { });
            var testMediaType = new AutoPublishedContentType(Guid.NewGuid(), 24, "TestMediaType", new PublishedPropertyType[] { });
            _mediaTypes = new Dictionary<string, PublishedContentType>
            {
                { type.Alias, type },
                { image.Alias, image },
                { testMediaType.Alias, testMediaType }
            };
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => _mediaTypes[alias];
        }

        private IMediaType MakeNewMediaType(IUser user, string text, int parentId = -1)
        {
            var mt = new MediaType(parentId) { Name = text, Alias = text, Thumbnail = "icon-folder", Icon = "icon-folder" };
            ServiceContext.MediaTypeService.Save(mt);
            return mt;
        }

        private IMedia MakeNewMedia(string name, IMediaType mediaType, IUser user, int parentId)
        {
            var m = ServiceContext.MediaService.CreateMediaWithIdentity(name, parentId, mediaType.Alias);
            return m;
        }

        //NOTE: This is "Without_Examine" too
        [Test]
        public void Get_Root_Docs()
        {
            var user = ServiceContext.UserService.GetUserById(0);
            var mType = MakeNewMediaType(user, "TestMediaType");
            var mRoot1 = MakeNewMedia("MediaRoot1", mType, user, -1);
            var mRoot2 = MakeNewMedia("MediaRoot2", mType, user, -1);
            var mChild1 = MakeNewMedia("Child1", mType, user, mRoot1.Id);
            var mChild2 = MakeNewMedia("Child2", mType, user, mRoot2.Id);

            var ctx = GetUmbracoContext("/test");
            var cache = new PublishedMediaCache(new XmlStore((XmlDocument) null, null, null, null), ServiceContext.MediaService, ServiceContext.UserService, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());
            var roots = cache.GetAtRoot();
            Assert.AreEqual(2, roots.Count());
            Assert.IsTrue(roots.Select(x => x.Id).ContainsAll(new[] {mRoot1.Id, mRoot2.Id}));

        }

        [Test]
        public void Get_Item_Without_Examine()
        {
            var user = ServiceContext.UserService.GetUserById(0);
            var mType = MakeNewMediaType(user, "TestMediaType");
            _mediaTypes[mType.Alias] = new PublishedContentType(mType, null);
            var mRoot = MakeNewMedia("MediaRoot", mType, user, -1);
            var mChild1 = MakeNewMedia("Child1", mType, user, mRoot.Id);

            //var publishedMedia = PublishedMediaTests.GetNode(mRoot.Id, GetUmbracoContext("/test", 1234));
            var umbracoContext = GetUmbracoContext("/test");
            var cache = new PublishedMediaCache(new XmlStore((XmlDocument)null, null, null, null), Current.Services.MediaService, Current.Services.UserService, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());
            var publishedMedia = cache.GetById(mRoot.Id);
            Assert.IsNotNull(publishedMedia);

            Assert.AreEqual(mRoot.Id, publishedMedia.Id);
            Assert.AreEqual(mRoot.CreateDate.ToString("dd/MM/yyyy HH:mm:ss"), publishedMedia.CreateDate.ToString("dd/MM/yyyy HH:mm:ss"));
            Assert.AreEqual(mRoot.CreatorId, publishedMedia.CreatorId);
            //Assert.AreEqual(mRoot.User.Name, publishedMedia.CreatorName);
            Assert.AreEqual(mRoot.ContentType.Alias, publishedMedia.ContentType.Alias);
            Assert.AreEqual(mRoot.ContentType.Id, publishedMedia.ContentType.Id);
            Assert.AreEqual(mRoot.Level, publishedMedia.Level);
            Assert.AreEqual(mRoot.Name, publishedMedia.Name);
            Assert.AreEqual(mRoot.Path, publishedMedia.Path);
            Assert.AreEqual(mRoot.SortOrder, publishedMedia.SortOrder);
            Assert.IsNull(publishedMedia.Parent);
        }

        [TestCase("id")]
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
        public void DictionaryDocument_Key()
        {
            var key = Guid.NewGuid();
            var dicDoc = GetDictionaryDocument(keyVal: key);
            DoAssert(dicDoc, keyVal: key);
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
            var ctx = GetUmbracoContext("/test");
            var key = Guid.NewGuid();

            var fields = new Dictionary<string, string>
            {
                {"__IndexType", "media"},
                {"__NodeId", "1234"},
                {"__NodeTypeAlias", Constants.Conventions.MediaTypes.Image},
                {"__Path", "-1,1234"},
                {"__nodeName", "Test"},
                {"id", "1234"},
                {"key", key.ToString()},
                {"urlName", "/media/test.jpg"},
                {"nodeType", "0"},
                {"sortOrder", "0"},
                {"level", "2"},
                {"nodeName", "Test"},
                {"nodeTypeAlias", Constants.Conventions.MediaTypes.Image},
                {"parentID", "-1"},
                {"path", "-1,1234"},
                {"updateDate", DateTime.Parse("2012-07-16T10:34:09").Ticks.ToString()},
                {"createDate", DateTime.Parse("2012-07-17T10:34:09").Ticks.ToString()},
                {"creatorID", "0"},
                {"creatorName", "Shannon"}
            };

            var result = new SearchResult("1234", 1, () => fields.ToDictionary(x => x.Key, x => new List<string> { x.Value }));

            var store = new PublishedMediaCache(new XmlStore((XmlDocument)null, null, null, null), ServiceContext.MediaService, ServiceContext.UserService, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());
            var doc = store.CreateFromCacheValues(store.ConvertFromSearchResult(result));

            DoAssert(doc, 1234, key, null, 0, "/media/test.jpg", "Image", 23, "Shannon", "Shannon", 0, 0, "-1,1234", DateTime.Parse("2012-07-17T10:34:09"), DateTime.Parse("2012-07-16T10:34:09"), 2);
            Assert.AreEqual(null, doc.Parent);
        }

        [Test]
        public void Convert_From_XPath_Navigator()
        {
            var ctx = GetUmbracoContext("/test");
            var key = Guid.NewGuid();

            var xmlDoc = GetMediaXml();
            ((XmlElement)xmlDoc.DocumentElement.FirstChild).SetAttribute("key", key.ToString());
            var navigator = xmlDoc.SelectSingleNode("/root/Image").CreateNavigator();
            var cache = new PublishedMediaCache(new XmlStore((XmlDocument)null, null, null, null), ServiceContext.MediaService, ServiceContext.UserService, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());
            var doc = cache.CreateFromCacheValues(cache.ConvertFromXPathNavigator(navigator, true));

            DoAssert(doc, 2000, key, null, 2, "image1", "Image", 23, "Shannon", "Shannon", 33, 33, "-1,2000", DateTime.Parse("2012-06-12T14:13:17"), DateTime.Parse("2012-07-20T18:50:43"), 1);
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
    <Image id=""2000"" parentID=""-1"" level=""1"" writerID=""33"" creatorID=""33"" nodeType=""2044"" template=""0"" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Image1"" urlName=""image1"" writerName=""Shannon"" creatorName=""Shannon"" path=""-1,2000"" isDoc="""">
        <file><![CDATA[/media/1234/image1.png]]></file>
        <Image id=""2001"" parentID=""2000"" level=""2"" writerID=""33"" creatorID=""33"" nodeType=""2044"" template=""0"" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Image1"" urlName=""image1"" writerName=""Shannon"" creatorName=""Shannon"" path=""-1,2000,2001"" isDoc="""">
            <file><![CDATA[/media/1234/image1.png]]></file>
        </Image>
        <Image id=""2002"" parentID=""2000"" level=""2"" writerID=""33"" creatorID=""33"" nodeType=""2044"" template=""0"" sortOrder=""2"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Image1"" urlName=""image1"" writerName=""Shannon"" creatorName=""Shannon"" path=""-1,2000,2002"" isDoc="""">
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
            Guid key,
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
                    {"key", key.ToString()},
                    {templateKey, "0"},
                    {"sortOrder", "44"},
                    {nodeNameKey, "Testing"},
                    {"urlName", "testing"},
                    {nodeTypeAliasKey, "myType"},
                    {"nodeType", "22"},
                    {"writerName", "Shannon"},
                    {"creatorName", "Shannon"},
                    {"writerID", "33"},
                    {"creatorID", "33"},
                    {pathKey, "1,2,3,4,5"},
                    {"createDate", "2012-01-02"},
                    {"updateDate", "2012-01-02"},
                    {"level", "3"},
                    {"parentID", parentId.ToString()}
                };
        }

        private DictionaryPublishedContent GetDictionaryDocument(
            string idKey = "id",
            string templateKey = "template",
            string nodeNameKey = "nodeName",
            string nodeTypeAliasKey = "nodeTypeAlias",
            string pathKey = "path",
            int idVal = 1234,
            Guid keyVal = default(Guid),
            int parentIdVal = 321,
            IEnumerable<IPublishedContent> children = null)
        {
            if (children == null)
                children = new List<IPublishedContent>();
            var dicDoc = new DictionaryPublishedContent(
            //the dictionary
            GetDictionary(idVal, keyVal, parentIdVal, idKey, templateKey, nodeNameKey, nodeTypeAliasKey, pathKey),
            //callback to get the parent
            d => new DictionaryPublishedContent(
                // the dictionary
                GetDictionary(parentIdVal, default(Guid), -1, idKey, templateKey, nodeNameKey, nodeTypeAliasKey, pathKey),
                // callback to get the parent: there is no parent
                a => null,
                // callback to get the children: we're not going to test this so ignore
                (dd, n) => new List<IPublishedContent>(),
                // callback to get a property
                (dd, a) => dd.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(a)),
                null, // cache provider
                ContentTypesCache,
                // no xpath
                null,
                // not from examine
                false),
            //callback to get the children
            (dd, n) => children,
            // callback to get a property
            (dd, a) => dd.Properties.FirstOrDefault(x => x.Alias.InvariantEquals(a)),
            null, // cache provider
            ContentTypesCache,
            // no xpath
            null,
            // not from examine
            false);
            return dicDoc;
        }

        private void DoAssert(
            DictionaryPublishedContent dicDoc,
            int idVal = 1234,
            Guid keyVal = default(Guid),
            int? templateIdVal = null,
            int sortOrderVal = 44,
            string urlNameVal = "testing",
            string nodeTypeAliasVal = "myType",
            int nodeTypeIdVal = 22,
            string writerNameVal = "Shannon",
            string creatorNameVal = "Shannon",
            int writerIdVal = 33,
            int creatorIdVal = 33,
            string pathVal = "1,2,3,4,5",
            DateTime? createDateVal = null,
            DateTime? updateDateVal = null,
            int levelVal = 3,
            int parentIdVal = 321)
        {
            if (!createDateVal.HasValue)
                createDateVal = DateTime.Parse("2012-01-02");
            if (!updateDateVal.HasValue)
                updateDateVal = DateTime.Parse("2012-01-02");

            DoAssert((IPublishedContent)dicDoc, idVal, keyVal, templateIdVal, sortOrderVal, urlNameVal, nodeTypeAliasVal, nodeTypeIdVal, writerNameVal,
                creatorNameVal, writerIdVal, creatorIdVal, pathVal, createDateVal, updateDateVal, levelVal);

            //now validate the parentId that has been parsed, this doesn't exist on the IPublishedContent
            Assert.AreEqual(parentIdVal, dicDoc.ParentId);
        }

        private void DoAssert(
            IPublishedContent doc,
            int idVal = 1234,
            Guid keyVal = default(Guid),
            int? templateIdVal = null,
            int sortOrderVal = 44,
            string urlNameVal = "testing",
            string nodeTypeAliasVal = "myType",
            int nodeTypeIdVal = 22,
            string writerNameVal = "Shannon",
            string creatorNameVal = "Shannon",
            int writerIdVal = 33,
            int creatorIdVal = 33,
            string pathVal = "1,2,3,4,5",
            DateTime? createDateVal = null,
            DateTime? updateDateVal = null,
            int levelVal = 3)
        {
            if (!createDateVal.HasValue)
                createDateVal = DateTime.Parse("2012-01-02");
            if (!updateDateVal.HasValue)
                updateDateVal = DateTime.Parse("2012-01-02");

            Assert.AreEqual(idVal, doc.Id);
            Assert.AreEqual(keyVal, doc.Key);
            Assert.AreEqual(templateIdVal, doc.TemplateId);
            Assert.AreEqual(sortOrderVal, doc.SortOrder);
            Assert.AreEqual(urlNameVal, doc.UrlSegment);
            Assert.AreEqual(nodeTypeAliasVal, doc.ContentType.Alias);
            Assert.AreEqual(nodeTypeIdVal, doc.ContentType.Id);
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

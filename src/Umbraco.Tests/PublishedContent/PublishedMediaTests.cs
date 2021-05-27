using Examine;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Examine;
using Umbraco.Tests.LegacyXmlPublishedCache;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Tests.UmbracoExamine;
using Umbraco.Web;

namespace Umbraco.Tests.PublishedContent
{
    /// <summary>
    /// Tests the typed extension methods on IPublishedContent using the DefaultPublishedMediaStore
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
    public class PublishedMediaTests : PublishedContentTestBase
    {
        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void Compose()
        {
            base.Compose();

            Composition.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>()
                .Clear()
                .Append<DefaultUrlSegmentProvider>();

            Composition.RegisterUnique<IUmbracoContextAccessor, TestUmbracoContextAccessor>();
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

        /// <summary>
        /// Shared with PublishMediaStoreTests
        /// </summary>
        /// <param name="id"></param>
        /// <param name="umbracoContext"></param>
        /// <returns></returns>
        internal IPublishedContent GetNode(int id, UmbracoContext umbracoContext)
        {
            var cache = new PublishedMediaCache(new XmlStore((XmlDocument)null, null, null, null),
                ServiceContext.MediaService, ServiceContext.UserService, new DictionaryAppCache(), ContentTypesCache,
                Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());
            var doc = cache.GetById(id);
            Assert.IsNotNull(doc);
            return doc;
        }

        private IPublishedContent GetNode(int id)
        {
            return GetNode(id, GetUmbracoContext("/test"));
        }

        [Test]
        public void Get_Property_Value_Uses_Converter()
        {
            var mType = MockedContentTypes.CreateImageMediaType("image2");
            //lets add an RTE to this
            mType.PropertyGroups.First().PropertyTypes.Add(
                new PropertyType("test", ValueStorageType.Nvarchar, "content")
                    {
                        Name = "Rich Text",
                        DataTypeId = -87 //tiny mce
                    });
            var existing = ServiceContext.MediaTypeService.GetAll();
            ServiceContext.MediaTypeService.Save(mType);
            var media = MockedMedia.CreateMediaImage(mType, -1);
            media.Properties["content"].SetValue("<div>This is some content</div>");
            ServiceContext.MediaService.Save(media);

            var publishedMedia = GetNode(media.Id);

            var propVal = publishedMedia.Value("content");
            Assert.IsInstanceOf<IHtmlString>(propVal);
            Assert.AreEqual("<div>This is some content</div>", propVal.ToString());

            var propVal2 = publishedMedia.Value<IHtmlString>("content");
            Assert.IsInstanceOf<IHtmlString>(propVal2);
            Assert.AreEqual("<div>This is some content</div>", propVal2.ToString());

            var propVal3 = publishedMedia.Value("Content");
            Assert.IsInstanceOf<IHtmlString>(propVal3);
            Assert.AreEqual("<div>This is some content</div>", propVal3.ToString());
        }

        [Test]
        public void Ensure_Children_Sorted_With_Examine()
        {
            var rebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(true)))
            using (indexer.ProcessNonAsync())
            {
                rebuilder.RegisterIndex(indexer.Name);
                rebuilder.Populate(indexer);

                var searcher = indexer.GetSearcher();
                var ctx = GetUmbracoContext("/test");
                var cache = new PublishedMediaCache(ServiceContext.MediaService, ServiceContext.UserService, searcher, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());

                //we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
                var publishedMedia = cache.GetById(1111);
                var rootChildren = publishedMedia.Children().ToArray();
                var currSort = 0;
                for (var i = 0; i < rootChildren.Count(); i++)
                {
                    Assert.GreaterOrEqual(rootChildren[i].SortOrder, currSort);
                    currSort = rootChildren[i].SortOrder;
                }
            }
        }

        [Test]
        public void Do_Not_Find_In_Recycle_Bin()
        {
            var rebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                //include unpublished content since this uses the 'internal' indexer, it's up to the media cache to filter
                validator: new ContentValueSetValidator(false)))
            using (indexer.ProcessNonAsync())
            {
                rebuilder.RegisterIndex(indexer.Name);
                rebuilder.Populate(indexer);

                var searcher = indexer.GetSearcher();
                var ctx = GetUmbracoContext("/test");
                var cache = new PublishedMediaCache(ServiceContext.MediaService, ServiceContext.UserService, searcher, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());

                //ensure it is found
                var publishedMedia = cache.GetById(3113);
                Assert.IsNotNull(publishedMedia);

                //move item to recycle bin
                var newXml = XElement.Parse(@"<node id='3113' key='5b3e46ab-3e37-4cfa-ab70-014234b5bd33' parentID='-21' level='1' writerID='0' nodeType='1032' template='0' sortOrder='2' createDate='2010-05-19T17:32:46' updateDate='2010-05-19T17:32:46' nodeName='Another Umbraco Image' urlName='acnestressscrub' writerName='Administrator' nodeTypeAlias='Image' path='-1,-21,3113'>
                    <data alias='umbracoFile'><![CDATA[/media/1234/blah.pdf]]></data>
                    <data alias='umbracoWidth'>115</data>
                    <data alias='umbracoHeight'>268</data>
                    <data alias='umbracoBytes'>10726</data>
                    <data alias='umbracoExtension'>jpg</data>
                </node>");
                indexer.IndexItems(new[]{ newXml.ConvertToValueSet("media") });


                //ensure it still exists in the index (raw examine search)
                var criteria = searcher.CreateQuery();
                var filter = criteria.Id(3113);
                var found = filter.Execute();
                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.TotalItemCount);

                //ensure it does not show up in the published media store
                var recycledMedia = cache.GetById(3113);
                Assert.IsNull(recycledMedia);

            }

        }

        [Test]
        public void Children_With_Examine()
        {
            var rebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(true)))
            using (indexer.ProcessNonAsync())
            {
                rebuilder.RegisterIndex(indexer.Name);
                rebuilder.Populate(indexer);

                var searcher = indexer.GetSearcher();
                var ctx = GetUmbracoContext("/test");
                var cache = new PublishedMediaCache(ServiceContext.MediaService, ServiceContext.UserService, searcher, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());

                //we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
                var publishedMedia = cache.GetById(1111);
                var rootChildren = publishedMedia.Children();
                Assert.IsTrue(rootChildren.Select(x => x.Id).ContainsAll(new[] { 2222, 1113, 1114, 1115, 1116 }));

                var publishedChild1 = cache.GetById(2222);
                var subChildren = publishedChild1.Children();
                Assert.IsTrue(subChildren.Select(x => x.Id).ContainsAll(new[] { 2112 }));
            }
        }

        [Test]
        public void Descendants_With_Examine()
        {
            var rebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(true)))
            using (indexer.ProcessNonAsync())
            {
                rebuilder.RegisterIndex(indexer.Name);
                rebuilder.Populate(indexer);

                var searcher = indexer.GetSearcher();
                var ctx = GetUmbracoContext("/test");
                var cache = new PublishedMediaCache(ServiceContext.MediaService, ServiceContext.UserService, searcher, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());

                //we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
                var publishedMedia = cache.GetById(1111);
                var rootDescendants = publishedMedia.Descendants();
                Assert.IsTrue(rootDescendants.Select(x => x.Id).ContainsAll(new[] { 2112, 2222, 1113, 1114, 1115, 1116 }));

                var publishedChild1 = cache.GetById(2222);
                var subDescendants = publishedChild1.Descendants();
                Assert.IsTrue(subDescendants.Select(x => x.Id).ContainsAll(new[] { 2112, 3113 }));
            }
        }

        [Test]
        public void DescendantsOrSelf_With_Examine()
        {
            var rebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(true)))
            using (indexer.ProcessNonAsync())
            {
                rebuilder.RegisterIndex(indexer.Name);
                rebuilder.Populate(indexer);

                var searcher = indexer.GetSearcher();
                var ctx = GetUmbracoContext("/test");
                var cache = new PublishedMediaCache(ServiceContext.MediaService, ServiceContext.UserService, searcher, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());

                //we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
                var publishedMedia = cache.GetById(1111);
                var rootDescendants = publishedMedia.DescendantsOrSelf();
                Assert.IsTrue(rootDescendants.Select(x => x.Id).ContainsAll(new[] { 1111, 2112, 2222, 1113, 1114, 1115, 1116 }));

                var publishedChild1 = cache.GetById(2222);
                var subDescendants = publishedChild1.DescendantsOrSelf();
                Assert.IsTrue(subDescendants.Select(x => x.Id).ContainsAll(new[] { 2222, 2112, 3113 }));
            }
        }

        [Test]
        public void Ancestors_With_Examine()
        {
            var rebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());


            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(true)))
            using (indexer.ProcessNonAsync())
            {
                rebuilder.RegisterIndex(indexer.Name);
                rebuilder.Populate(indexer);

                var ctx = GetUmbracoContext("/test");
                var searcher = indexer.GetSearcher();
                var cache = new PublishedMediaCache(ServiceContext.MediaService, ServiceContext.UserService, searcher, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());

                //we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
                var publishedMedia = cache.GetById(3113);
                var ancestors = publishedMedia.Ancestors();
                Assert.IsTrue(ancestors.Select(x => x.Id).ContainsAll(new[] { 2112, 2222, 1111 }));
            }

        }

        [Test]
        public void AncestorsOrSelf_With_Examine()
        {
            var rebuilder = IndexInitializer.GetMediaIndexRebuilder(Factory.GetInstance<PropertyEditorCollection>(), IndexInitializer.GetMockMediaService());

            using (var luceneDir = new RandomIdRamDirectory())
            using (var indexer = IndexInitializer.GetUmbracoIndexer(ProfilingLogger, luceneDir,
                validator: new ContentValueSetValidator(true)))
            using (indexer.ProcessNonAsync())
            {
                rebuilder.RegisterIndex(indexer.Name);
                rebuilder.Populate(indexer);


                var ctx = GetUmbracoContext("/test");
                var searcher = indexer.GetSearcher();
                var cache = new PublishedMediaCache(ServiceContext.MediaService, ServiceContext.UserService, searcher, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());

                //we are using the media.xml media to test the examine results implementation, see the media.xml file in the ExamineHelpers namespace
                var publishedMedia = cache.GetById(3113);
                var ancestors = publishedMedia.AncestorsOrSelf();
                Assert.IsTrue(ancestors.Select(x => x.Id).ContainsAll(new[] { 3113, 2112, 2222, 1111 }));
            }
        }

        [Test]
        public void Children_Without_Examine()
        {
            var user = ServiceContext.UserService.GetUserById(0);
            var mType = MakeNewMediaType(user, "TestMediaType");
            var mRoot = MakeNewMedia("MediaRoot", mType, user, -1);

            var mChild1 = MakeNewMedia("Child1", mType, user, mRoot.Id);
            var mChild2 = MakeNewMedia("Child2", mType, user, mRoot.Id);
            var mChild3 = MakeNewMedia("Child3", mType, user, mRoot.Id);

            var mSubChild1 = MakeNewMedia("SubChild1", mType, user, mChild1.Id);
            var mSubChild2 = MakeNewMedia("SubChild2", mType, user, mChild1.Id);
            var mSubChild3 = MakeNewMedia("SubChild3", mType, user, mChild1.Id);

            var publishedMedia = GetNode(mRoot.Id);
            var rootChildren = publishedMedia.Children();
            Assert.IsTrue(rootChildren.Select(x => x.Id).ContainsAll(new[] { mChild1.Id, mChild2.Id, mChild3.Id }));

            var publishedChild1 = GetNode(mChild1.Id);
            var subChildren = publishedChild1.Children();
            Assert.IsTrue(subChildren.Select(x => x.Id).ContainsAll(new[] { mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));
        }

        [Test]
        public void Descendants_Without_Examine()
        {
            var user = ServiceContext.UserService.GetUserById(0);
            var mType = MakeNewMediaType(user, "TestMediaType");
            var mRoot = MakeNewMedia("MediaRoot", mType, user, -1);

            var mChild1 = MakeNewMedia("Child1", mType, user, mRoot.Id);
            var mChild2 = MakeNewMedia("Child2", mType, user, mRoot.Id);
            var mChild3 = MakeNewMedia("Child3", mType, user, mRoot.Id);

            var mSubChild1 = MakeNewMedia("SubChild1", mType, user, mChild1.Id);
            var mSubChild2 = MakeNewMedia("SubChild2", mType, user, mChild1.Id);
            var mSubChild3 = MakeNewMedia("SubChild3", mType, user, mChild1.Id);

            var publishedMedia = GetNode(mRoot.Id);
            var rootDescendants = publishedMedia.Descendants();
            Assert.IsTrue(rootDescendants.Select(x => x.Id).ContainsAll(new[] { mChild1.Id, mChild2.Id, mChild3.Id, mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));

            var publishedChild1 = GetNode(mChild1.Id);
            var subDescendants = publishedChild1.Descendants();
            Assert.IsTrue(subDescendants.Select(x => x.Id).ContainsAll(new[] { mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));
        }

        [Test]
        public void DescendantsOrSelf_Without_Examine()
        {
            var user = ServiceContext.UserService.GetUserById(0);
            var mType = MakeNewMediaType(user, "TestMediaType");
            var mRoot = MakeNewMedia("MediaRoot", mType, user, -1);

            var mChild1 = MakeNewMedia("Child1", mType, user, mRoot.Id);
            var mChild2 = MakeNewMedia("Child2", mType, user, mRoot.Id);
            var mChild3 = MakeNewMedia("Child3", mType, user, mRoot.Id);

            var mSubChild1 = MakeNewMedia("SubChild1", mType, user, mChild1.Id);
            var mSubChild2 = MakeNewMedia("SubChild2", mType, user, mChild1.Id);
            var mSubChild3 = MakeNewMedia("SubChild3", mType, user, mChild1.Id);

            var publishedMedia = GetNode(mRoot.Id);
            var rootDescendantsOrSelf = publishedMedia.DescendantsOrSelf();
            Assert.IsTrue(rootDescendantsOrSelf.Select(x => x.Id).ContainsAll(
                new[] { mRoot.Id, mChild1.Id, mChild2.Id, mChild3.Id, mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));

            var publishedChild1 = GetNode(mChild1.Id);
            var subDescendantsOrSelf = publishedChild1.DescendantsOrSelf();
            Assert.IsTrue(subDescendantsOrSelf.Select(x => x.Id).ContainsAll(
                new[] { mChild1.Id, mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));
        }

        [Test]
        public void Parent_Without_Examine()
        {
            var user = ServiceContext.UserService.GetUserById(0);
            var mType = MakeNewMediaType(user, "TestMediaType");
            var mRoot = MakeNewMedia("MediaRoot", mType, user, -1);

            var mChild1 = MakeNewMedia("Child1", mType, user, mRoot.Id);
            var mChild2 = MakeNewMedia("Child2", mType, user, mRoot.Id);
            var mChild3 = MakeNewMedia("Child3", mType, user, mRoot.Id);

            var mSubChild1 = MakeNewMedia("SubChild1", mType, user, mChild1.Id);
            var mSubChild2 = MakeNewMedia("SubChild2", mType, user, mChild1.Id);
            var mSubChild3 = MakeNewMedia("SubChild3", mType, user, mChild1.Id);

            var publishedRoot = GetNode(mRoot.Id);
            Assert.AreEqual(null, publishedRoot.Parent);

            var publishedChild1 = GetNode(mChild1.Id);
            Assert.AreEqual(mRoot.Id, publishedChild1.Parent.Id);

            var publishedSubChild1 = GetNode(mSubChild1.Id);
            Assert.AreEqual(mChild1.Id, publishedSubChild1.Parent.Id);
        }

        [Test]
        public void Ancestors_Without_Examine()
        {
            var user = ServiceContext.UserService.GetUserById(0);
            var mType = MakeNewMediaType(user, "TestMediaType");
            var mRoot = MakeNewMedia("MediaRoot", mType, user, -1);

            var mChild1 = MakeNewMedia("Child1", mType, user, mRoot.Id);
            var mChild2 = MakeNewMedia("Child2", mType, user, mRoot.Id);
            var mChild3 = MakeNewMedia("Child3", mType, user, mRoot.Id);

            var mSubChild1 = MakeNewMedia("SubChild1", mType, user, mChild1.Id);
            var mSubChild2 = MakeNewMedia("SubChild2", mType, user, mChild1.Id);
            var mSubChild3 = MakeNewMedia("SubChild3", mType, user, mChild1.Id);

            var publishedSubChild1 = GetNode(mSubChild1.Id);
            Assert.IsTrue(publishedSubChild1.Ancestors().Select(x => x.Id).ContainsAll(new[] { mChild1.Id, mRoot.Id }));
        }

        [Test]
        public void AncestorsOrSelf_Without_Examine()
        {
            var user = ServiceContext.UserService.GetUserById(0);
            var mType = MakeNewMediaType(user, "TestMediaType");
            var mRoot = MakeNewMedia("MediaRoot", mType, user, -1);

            var mChild1 = MakeNewMedia("Child1", mType, user, mRoot.Id);
            var mChild2 = MakeNewMedia("Child2", mType, user, mRoot.Id);
            var mChild3 = MakeNewMedia("Child3", mType, user, mRoot.Id);

            var mSubChild1 = MakeNewMedia("SubChild1", mType, user, mChild1.Id);
            var mSubChild2 = MakeNewMedia("SubChild2", mType, user, mChild1.Id);
            var mSubChild3 = MakeNewMedia("SubChild3", mType, user, mChild1.Id);

            var publishedSubChild1 = GetNode(mSubChild1.Id);
            Assert.IsTrue(publishedSubChild1.AncestorsOrSelf().Select(x => x.Id).ContainsAll(
                new[] { mSubChild1.Id, mChild1.Id, mRoot.Id }));
        }

        [Test]
        public void Convert_From_Standard_Xml()
        {
            var nodeId = 2112;

            var xml = XElement.Parse(@"<Image id=""2112"" version=""5b3e46ab-3e37-4cfa-ab70-014234b5bd39"" parentID=""2222"" level=""3"" writerID=""0"" nodeType=""1032"" template=""0"" sortOrder=""1"" createDate=""2010-05-19T17:32:46"" updateDate=""2010-05-19T17:32:46"" nodeName=""Sam's Umbraco Image"" urlName=""acnestressscrub"" writerName=""Administrator"" nodeTypeAlias=""Image"" path=""-1,1111,2222,2112"" isDoc="""">
                <umbracoFile><![CDATA[/media/1234/blah.pdf]]></umbracoFile>
                <umbracoWidth>115</umbracoWidth>
                <umbracoHeight>268</umbracoHeight>
                <umbracoBytes>10726</umbracoBytes>
                <umbracoExtension>jpg</umbracoExtension>
                <Image id=""3113"" version=""5b3e46ab-3e37-4cfa-ab70-014234b5bd33"" parentID=""2112"" level=""4"" writerID=""0"" nodeType=""1032"" template=""0"" sortOrder=""2"" createDate=""2010-05-19T17:32:46"" updateDate=""2010-05-19T17:32:46"" nodeName=""Another Umbraco Image"" urlName=""acnestressscrub"" writerName=""Administrator"" nodeTypeAlias=""Image"" path=""-1,1111,2222,2112,3113"" isDoc="""">
                    <umbracoFile><![CDATA[/media/1234/blah.pdf]]></umbracoFile>
                    <umbracoWidth>115</umbracoWidth>
                    <umbracoHeight>268</umbracoHeight>
                    <umbracoBytes>10726</umbracoBytes>
                    <umbracoExtension>jpg</umbracoExtension>
                </Image>
            </Image>");
            var node = xml.DescendantsAndSelf("Image").Single(x => (int)x.Attribute("id") == nodeId);

            var publishedMedia = new PublishedMediaCache(new XmlStore((XmlDocument)null, null, null, null), ServiceContext.MediaService, ServiceContext.UserService, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());

            var nav = node.CreateNavigator();

            var converted = publishedMedia.CreateFromCacheValues(
                publishedMedia.ConvertFromXPathNodeIterator(nav.Select("/Image"), nodeId));

            Assert.AreEqual(nodeId, converted.Id);
            Assert.AreEqual(3, converted.Level);
            Assert.AreEqual(1, converted.SortOrder);
            Assert.AreEqual("Sam's Umbraco Image", converted.Name);
            Assert.AreEqual("-1,1111,2222,2112", converted.Path);
        }

        [Test]
        public void Detects_Error_In_Xml()
        {
            var errorXml = new XElement("error", string.Format("No media is maching '{0}'", 1234));
            var nav = errorXml.CreateNavigator();

            var publishedMedia = new PublishedMediaCache(new XmlStore((XmlDocument)null, null, null, null), ServiceContext.MediaService, ServiceContext.UserService, new DictionaryAppCache(), ContentTypesCache, Factory.GetInstance<IEntityXmlSerializer>(), Factory.GetInstance<IUmbracoContextAccessor>());
            var converted = publishedMedia.ConvertFromXPathNodeIterator(nav.Select("/"), 1234);

            Assert.IsNull(converted);
        }
    }
}

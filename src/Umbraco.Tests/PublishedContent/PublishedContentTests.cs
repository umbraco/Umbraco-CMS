using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Core.Composing;
using Moq;
using Newtonsoft.Json;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Templates;
using Umbraco.Web.Models;

namespace Umbraco.Tests.PublishedContent
{
    /// <summary>
    /// Tests the methods on IPublishedContent using the DefaultPublishedContentStore
    /// </summary>
    [TestFixture]
    [UmbracoTest(TypeLoader = UmbracoTestOptions.TypeLoader.PerFixture)]
    public class PublishedContentTests : PublishedContentTestBase
    {
        protected override void Compose()
        {
            base.Compose();
            _publishedSnapshotAccessorMock = new Mock<IPublishedSnapshotAccessor>();
            Composition.RegisterUnique<IPublishedSnapshotAccessor>(_publishedSnapshotAccessorMock.Object);

            Composition.RegisterUnique<IPublishedModelFactory>(f => new PublishedModelFactory(f.GetInstance<TypeLoader>().GetTypes<PublishedContentModel>()));
            Composition.RegisterUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();
            Composition.RegisterUnique<IPublishedValueFallback, PublishedValueFallback>();

            var logger = Mock.Of<ILogger>();
            var mediaService = Mock.Of<IMediaService>();
            var contentTypeBaseServiceProvider = Mock.Of<IContentTypeBaseServiceProvider>();
            var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
            var imageSourceParser = new HtmlImageSourceParser(umbracoContextAccessor);
            var pastedImages = new RichTextEditorPastedImages(umbracoContextAccessor, logger, mediaService, contentTypeBaseServiceProvider);
            var linkParser = new HtmlLocalLinkParser(umbracoContextAccessor);

            var dataTypeService = new TestObjects.TestDataTypeService(
                new DataType(new VoidEditor(logger)) { Id = 1 },
                new DataType(new TrueFalsePropertyEditor(logger)) { Id = 1001 },
                new DataType(new RichTextPropertyEditor(logger, umbracoContextAccessor, imageSourceParser, linkParser, pastedImages, Mock.Of<IImageUrlGenerator>(), Mock.Of<IHtmlSanitizer>())) { Id = 1002 },
                new DataType(new IntegerPropertyEditor(logger)) { Id = 1003 },
                new DataType(new TextboxPropertyEditor(logger)) { Id = 1004 },
                new DataType(new MediaPickerPropertyEditor(logger)) { Id = 1005 });
            Composition.RegisterUnique<IDataTypeService>(f => dataTypeService);
        }

        protected override void Initialize()
        {
            base.Initialize();

            var factory = Factory.GetInstance<IPublishedContentTypeFactory>() as PublishedContentTypeFactory;

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically
            // when they are requested, but we must declare those that we
            // explicitely want to be here...

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
            {
                // AutoPublishedContentType will auto-generate other properties
                yield return factory.CreatePropertyType(contentType, "umbracoNaviHide", 1001);
                yield return factory.CreatePropertyType(contentType, "selectedNodes", 1);
                yield return factory.CreatePropertyType(contentType, "umbracoUrlAlias", 1);
                yield return factory.CreatePropertyType(contentType, "content", 1002);
                yield return factory.CreatePropertyType(contentType, "testRecursive", 1);
            }

            var compositionAliases = new[] { "MyCompositionAlias" };
            var anythingType = new AutoPublishedContentType(Guid.NewGuid(), 0, "anything", compositionAliases, CreatePropertyTypes);
            var homeType = new AutoPublishedContentType(Guid.NewGuid(), 0, "home", compositionAliases, CreatePropertyTypes);
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => alias.InvariantEquals("home") ? homeType : anythingType;
        }

        protected override TypeLoader CreateTypeLoader(IAppPolicyCache runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger)
        {
            var pluginManager = base.CreateTypeLoader(runtimeCache, globalSettings, logger);

            // this is so the model factory looks into the test assembly
            pluginManager.AssembliesToScan = pluginManager.AssembliesToScan
                .Union(new[] { typeof(PublishedContentTests).Assembly })
                .ToList();

            return pluginManager;
        }

        private readonly Guid _node1173Guid = Guid.NewGuid();
        private Mock<IPublishedSnapshotAccessor> _publishedSnapshotAccessorMock;

        protected override string GetXmlContent(int templateId)
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>
<!ELEMENT CustomDocument ANY>
<!ATTLIST CustomDocument id ID #REQUIRED>
]>
<root id=""-1"">
    <Home id=""1046"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc="""">
        <content><![CDATA[]]></content>
        <umbracoUrlAlias><![CDATA[this/is/my/alias, anotheralias]]></umbracoUrlAlias>
        <umbracoNaviHide>1</umbracoNaviHide>
        <testRecursive><![CDATA[This is the recursive val]]></testRecursive>
        <Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc="""" key=""" + _node1173Guid + @""">
            <content><![CDATA[<div>This is some content</div>]]></content>
            <umbracoUrlAlias><![CDATA[page2/alias, 2ndpagealias]]></umbracoUrlAlias>
            <testRecursive><![CDATA[]]></testRecursive>
            <Home id=""1174"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Sub2"" urlName=""sub2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1174"" isDoc="""">
                <content><![CDATA[]]></content>
                <umbracoUrlAlias><![CDATA[only/one/alias]]></umbracoUrlAlias>
                <creatorName><![CDATA[Custom data with same property name as the member name]]></creatorName>
                <testRecursive><![CDATA[]]></testRecursive>
            </Home>
			<CustomDocument id=""117"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2018-07-18T10:06:37"" updateDate=""2018-07-18T10:06:37"" nodeName=""custom sub 1"" urlName=""custom-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,117"" isDoc="""" />
			<CustomDocument id=""1177"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""custom sub 1"" urlName=""custom-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1177"" isDoc="""" />
			<CustomDocument id=""1178"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""custom sub 2"" urlName=""custom-sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1178"" isDoc="""">
				<CustomDocument id=""1179"" parentID=""1178"" level=""4"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""custom sub sub 1"" urlName=""custom-sub-sub-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1178,1179"" isDoc="""" />
			</CustomDocument>
			<Home id=""1176"" parentID=""1173"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""5"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Sub 3"" urlName=""sub-3"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173,1176"" isDoc="""" key=""CDB83BBC-A83B-4BA6-93B8-AADEF67D3C09"">
                <content><![CDATA[]]></content>
                <umbracoNaviHide>1</umbracoNaviHide>
            </Home>
        </Home>
        <Home id=""1175"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:08:01"" updateDate=""2012-07-20T18:49:32"" nodeName=""Sub 2"" urlName=""sub-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1175"" isDoc=""""><content><![CDATA[]]></content>
        </Home>
        <CustomDocument id=""4444"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,4444"" isDoc="""">
            <selectedNodes><![CDATA[1172,1176,1173]]></selectedNodes>
        </CustomDocument>
    </Home>
    <CustomDocument id=""1172"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1234"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""Test"" urlName=""test-page"" writerName=""admin"" creatorName=""admin"" path=""-1,1172"" isDoc="""" />
</root>";
        }

        internal IPublishedContent GetNode(int id)
        {
            var ctx = GetUmbracoContext("/test");
            var doc = ctx.Content.GetById(id);
            Assert.IsNotNull(doc);
            return doc;
        }

        [Test]
        public void GetNodeByIds()
        {
            var ctx = GetUmbracoContext("/test");
            var contentById = ctx.Content.GetById(1173);
            Assert.IsNotNull(contentById);
            var contentByGuid = ctx.Content.GetById(_node1173Guid);
            Assert.IsNotNull(contentByGuid);
            Assert.AreEqual(contentById.Id, contentByGuid.Id);
            Assert.AreEqual(contentById.Key, contentByGuid.Key);

            contentById = ctx.Content.GetById(666);
            Assert.IsNull(contentById);
            contentByGuid = ctx.Content.GetById(Guid.NewGuid());
            Assert.IsNull(contentByGuid);
        }

        [Test]
        public void Is_Last_From_Where_Filter_Dynamic_Linq()
        {
            var doc = GetNode(1173);

            var items = doc.Children().Where(x => x.IsVisible()).ToIndexedArray();

            foreach (var item in items)
            {
                if (item.Content.Id != 1178)
                {
                    Assert.IsFalse(item.IsLast());
                }
                else
                {
                    Assert.IsTrue(item.IsLast());
                }
            }
        }

        [Test]
        public void Is_Last_From_Where_Filter()
        {
            var doc = GetNode(1173);

            var items = doc
                .Children()
                .Where(x => x.IsVisible())
                .ToIndexedArray();

            Assert.AreEqual(4, items.Length);

            foreach (var d in items)
            {
                switch (d.Content.Id)
                {
                    case 1174:
                        Assert.IsTrue(d.IsFirst());
                        Assert.IsFalse(d.IsLast());
                        break;
                    case 117:
                        Assert.IsFalse(d.IsFirst());
                        Assert.IsFalse(d.IsLast());
                        break;
                    case 1177:
                        Assert.IsFalse(d.IsFirst());
                        Assert.IsFalse(d.IsLast());
                        break;
                    case 1178:
                        Assert.IsFalse(d.IsFirst());
                        Assert.IsTrue(d.IsLast());
                        break;
                    default:
                        Assert.Fail("Invalid id.");
                        break;
                }
            }
        }

        [PublishedModel("Home")]
        internal class Home : PublishedContentModel
        {
            public Home(IPublishedContent content)
                : base(content)
            {}
        }

        [PublishedModel("anything")]
        internal class Anything : PublishedContentModel
        {
            public Anything(IPublishedContent content)
                : base(content)
            { }
        }

        [Test]
        public void Is_Last_From_Where_Filter2()
        {
            var doc = GetNode(1173);
            var ct = doc.ContentType;

            var items = doc.Children()
                .Select(x => x.CreateModel()) // linq, returns IEnumerable<IPublishedContent>

                // only way around this is to make sure every IEnumerable<T> extension
                // explicitely returns a PublishedContentSet, not an IEnumerable<T>

                .OfType<Home>() // ours, return IEnumerable<Home> (actually a PublishedContentSet<Home>)
                .Where(x => x.IsVisible()) // so, here it's linq again :-(
                .ToIndexedArray(); // so, we need that one for the test to pass

            Assert.AreEqual(1, items.Length);

            foreach (var d in items)
            {
                switch (d.Content.Id)
                {
                    case 1174:
                        Assert.IsTrue(d.IsFirst());
                        Assert.IsTrue(d.IsLast());
                        break;
                    default:
                        Assert.Fail("Invalid id.");
                        break;
                }
            }
        }

        [Test]
        public void Is_Last_From_Take()
        {
            var doc = GetNode(1173);

            var items = doc.Children().Take(4).ToIndexedArray();

            foreach (var item in items)
            {
                if (item.Content.Id != 1178)
                {
                    Assert.IsFalse(item.IsLast());
                }
                else
                {
                    Assert.IsTrue(item.IsLast());
                }
            }
        }

        [Test]
        public void Is_Last_From_Skip()
        {
            var doc = GetNode(1173);

            foreach (var d in doc.Children().Skip(1).ToIndexedArray())
            {
                if (d.Content.Id != 1176)
                {
                    Assert.IsFalse(d.IsLast());
                }
                else
                {
                    Assert.IsTrue(d.IsLast());
                }
            }
        }

        [Test]
        public void Is_Last_From_Concat()
        {
            var doc = GetNode(1173);

            var items = doc.Children()
                .Concat(new[] { GetNode(1175), GetNode(4444) })
                .ToIndexedArray();

            foreach (var item in items)
            {
                if (item.Content.Id != 4444)
                {
                    Assert.IsFalse(item.IsLast());
                }
                else
                {
                    Assert.IsTrue(item.IsLast());
                }
            }
        }

        [Test]
        public void Descendants_Ordered_Properly()
        {
            var doc = GetNode(1046);

            var expected = new[] { 1046, 1173, 1174, 117, 1177, 1178, 1179, 1176, 1175, 4444, 1172 };
            var exindex = 0;

            // must respect the XPath descendants-or-self axis!
            foreach (var d in doc.DescendantsOrSelf())
                Assert.AreEqual(expected[exindex++], d.Id);
        }

        [Test]
        public void Get_Property_Value_Recursive()
        {
            var doc = GetNode(1174);
            var rVal = doc.Value("testRecursive", fallback: Fallback.ToAncestors);
            var nullVal = doc.Value("DoNotFindThis", fallback: Fallback.ToAncestors);
            Assert.AreEqual("This is the recursive val", rVal);
            Assert.AreEqual(null, nullVal);
        }

        [Test]
        public void Get_Property_Value_Uses_Converter()
        {
            var doc = GetNode(1173);

            var propVal = doc.Value("content");
            Assert.IsInstanceOf(typeof(IHtmlString), propVal);
            Assert.AreEqual("<div>This is some content</div>", propVal.ToString());

            var propVal2 = doc.Value<IHtmlString>("content");
            Assert.IsInstanceOf(typeof(IHtmlString), propVal2);
            Assert.AreEqual("<div>This is some content</div>", propVal2.ToString());

            var propVal3 = doc.Value("Content");
            Assert.IsInstanceOf(typeof(IHtmlString), propVal3);
            Assert.AreEqual("<div>This is some content</div>", propVal3.ToString());
        }

        [Test]
        public void Complex_Linq()
        {
            var doc = GetNode(1173);

            var result = doc.Ancestors().OrderBy(x => x.Level)
                .Single()
                .Descendants()
                .FirstOrDefault(x => x.Value<string>("selectedNodes", defaultValue: "").Split(',').Contains("1173"));

            Assert.IsNotNull(result);
        }

        [Test]
        public void Children_GroupBy_DocumentTypeAlias()
        {
            var home = new AutoPublishedContentType(Guid.NewGuid(), 22, "Home", new PublishedPropertyType[] { });
            var custom = new AutoPublishedContentType(Guid.NewGuid(), 23, "CustomDocument", new PublishedPropertyType[] { });
            var contentTypes = new Dictionary<string, PublishedContentType>
            {
                { home.Alias, home },
                { custom.Alias, custom }
            };
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => contentTypes[alias];

            var doc = GetNode(1046);

            var found1 = doc.Children().GroupBy(x => x.ContentType.Alias).ToArray();

            Assert.AreEqual(2, found1.Length);
            Assert.AreEqual(2, found1.Single(x => x.Key.ToString() == "Home").Count());
            Assert.AreEqual(1, found1.Single(x => x.Key.ToString() == "CustomDocument").Count());
        }

        [Test]
        public void Children_Where_DocumentTypeAlias()
        {
            var home = new AutoPublishedContentType(Guid.NewGuid(), 22, "Home", new PublishedPropertyType[] { });
            var custom = new AutoPublishedContentType(Guid.NewGuid(), 23, "CustomDocument", new PublishedPropertyType[] { });
            var contentTypes = new Dictionary<string, PublishedContentType>
            {
                { home.Alias, home },
                { custom.Alias, custom }
            };
            ContentTypesCache.GetPublishedContentTypeByAlias = alias => contentTypes[alias];

            var doc = GetNode(1046);

            var found1 = doc.Children().Where(x => x.ContentType.Alias == "CustomDocument");
            var found2 = doc.Children().Where(x => x.ContentType.Alias == "Home");

            Assert.AreEqual(1, found1.Count());
            Assert.AreEqual(2, found2.Count());
        }

        [Test]
        public void Children_Order_By_Update_Date()
        {
            var doc = GetNode(1173);

            var ordered = doc.Children().OrderBy(x => x.UpdateDate);

            var correctOrder = new[] { 1178, 1177, 1174, 1176 };
            for (var i = 0; i < correctOrder.Length; i++)
            {
                Assert.AreEqual(correctOrder[i], ordered.ElementAt(i).Id);
            }

        }

        [Test]
        public void FirstChild()
        {
            var doc = GetNode(1173); // has child nodes
            Assert.IsNotNull(doc.FirstChild());
            Assert.IsNotNull(doc.FirstChild(x => true));
            Assert.IsNotNull(doc.FirstChild<IPublishedContent>());

            doc = GetNode(1175); // does not have child nodes
            Assert.IsNull(doc.FirstChild());
            Assert.IsNull(doc.FirstChild(x => true));
            Assert.IsNull(doc.FirstChild<IPublishedContent>());
        }

        [Test]
        public void FirstChildAsT()
        {
            var doc = GetNode(1046); // has child nodes

            var model = doc.FirstChild<Home>(x => true); // predicate

            Assert.IsNotNull(model);
            Assert.IsTrue(model.Id == 1173);
            Assert.IsInstanceOf<Home>(model);
            Assert.IsInstanceOf<IPublishedContent>(model);

            doc = GetNode(1175); // does not have child nodes
            Assert.IsNull(doc.FirstChild<Anything>());
            Assert.IsNull(doc.FirstChild<Anything>(x => true));
        }

        [Test]
        public void IsComposedOf()
        {
            var doc = GetNode(1173);

            var isComposedOf = doc.IsComposedOf("MyCompositionAlias");

            Assert.IsTrue(isComposedOf);
        }

        [Test]
        public void HasProperty()
        {
            var doc = GetNode(1173);

            var hasProp = doc.HasProperty(Constants.Conventions.Content.UrlAlias);

            Assert.IsTrue(hasProp);
        }

        [Test]
        public void HasValue()
        {
            var doc = GetNode(1173);

            var hasValue = doc.HasValue(Constants.Conventions.Content.UrlAlias);
            var noValue = doc.HasValue("blahblahblah");

            Assert.IsTrue(hasValue);
            Assert.IsFalse(noValue);
        }

        [Test]
        public void Ancestors_Where_Visible()
        {
            var doc = GetNode(1174);

            var whereVisible = doc.Ancestors().Where(x => x.IsVisible());
            Assert.AreEqual(1, whereVisible.Count());

        }

        [Test]
        public void Visible()
        {
            var hidden = GetNode(1046);
            var visible = GetNode(1173);

            Assert.IsFalse(hidden.IsVisible());
            Assert.IsTrue(visible.IsVisible());
        }

        [Test]
        public void Ancestor_Or_Self()
        {
            var doc = GetNode(1173);

            var result = doc.AncestorOrSelf();

            Assert.IsNotNull(result);

            // ancestor-or-self has to be self!
            Assert.AreEqual(1173, result.Id);
        }

        [Test]
        public void U4_4559()
        {
            var doc = GetNode(1174);
            var result = doc.AncestorOrSelf(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1046, result.Id);
        }

        [Test]
        public void Ancestors_Or_Self()
        {
            var doc = GetNode(1174);

            var result = doc.AncestorsOrSelf().ToArray();

            Assert.IsNotNull(result);

            Assert.AreEqual(3, result.Length);
            Assert.IsTrue(result.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1174, 1173, 1046 }));
        }

        [Test]
        public void Ancestors()
        {
            var doc = GetNode(1174);

            var result = doc.Ancestors().ToArray();

            Assert.IsNotNull(result);

            Assert.AreEqual(2, result.Length);
            Assert.IsTrue(result.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1173, 1046 }));
        }

        [Test]
        public void IsAncestor()
        {
            // Structure:
            // - Root : 1046 (no parent)
            // -- Home: 1173 (parent 1046)
            // -- Custom Doc: 1178 (parent 1173)
            // --- Custom Doc2: 1179 (parent: 1178)
            // -- Custom Doc4: 117 (parent 1173)
            // - Custom Doc3: 1172 (no parent)

            var home = GetNode(1173);
            var root = GetNode(1046);
            var customDoc = GetNode(1178);
            var customDoc2 = GetNode(1179);
            var customDoc3 = GetNode(1172);
            var customDoc4 = GetNode(117);

            Assert.IsTrue(root.IsAncestor(customDoc4));
            Assert.IsFalse(root.IsAncestor(customDoc3));
            Assert.IsTrue(root.IsAncestor(customDoc2));
            Assert.IsTrue(root.IsAncestor(customDoc));
            Assert.IsTrue(root.IsAncestor(home));
            Assert.IsFalse(root.IsAncestor(root));

            Assert.IsTrue(home.IsAncestor(customDoc4));
            Assert.IsFalse(home.IsAncestor(customDoc3));
            Assert.IsTrue(home.IsAncestor(customDoc2));
            Assert.IsTrue(home.IsAncestor(customDoc));
            Assert.IsFalse(home.IsAncestor(home));
            Assert.IsFalse(home.IsAncestor(root));

            Assert.IsFalse(customDoc.IsAncestor(customDoc4));
            Assert.IsFalse(customDoc.IsAncestor(customDoc3));
            Assert.IsTrue(customDoc.IsAncestor(customDoc2));
            Assert.IsFalse(customDoc.IsAncestor(customDoc));
            Assert.IsFalse(customDoc.IsAncestor(home));
            Assert.IsFalse(customDoc.IsAncestor(root));

            Assert.IsFalse(customDoc2.IsAncestor(customDoc4));
            Assert.IsFalse(customDoc2.IsAncestor(customDoc3));
            Assert.IsFalse(customDoc2.IsAncestor(customDoc2));
            Assert.IsFalse(customDoc2.IsAncestor(customDoc));
            Assert.IsFalse(customDoc2.IsAncestor(home));
            Assert.IsFalse(customDoc2.IsAncestor(root));

            Assert.IsFalse(customDoc3.IsAncestor(customDoc3));
        }

        [Test]
        public void IsAncestorOrSelf()
        {
            // Structure:
            // - Root : 1046 (no parent)
            // -- Home: 1173 (parent 1046)
            // -- Custom Doc: 1178 (parent 1173)
            // --- Custom Doc2: 1179 (parent: 1178)
            // -- Custom Doc4: 117 (parent 1173)
            // - Custom Doc3: 1172 (no parent)

            var home = GetNode(1173);
            var root = GetNode(1046);
            var customDoc = GetNode(1178);
            var customDoc2 = GetNode(1179);
            var customDoc3 = GetNode(1172);
            var customDoc4 = GetNode(117);

            Assert.IsTrue(root.IsAncestorOrSelf(customDoc4));
            Assert.IsFalse(root.IsAncestorOrSelf(customDoc3));
            Assert.IsTrue(root.IsAncestorOrSelf(customDoc2));
            Assert.IsTrue(root.IsAncestorOrSelf(customDoc));
            Assert.IsTrue(root.IsAncestorOrSelf(home));
            Assert.IsTrue(root.IsAncestorOrSelf(root));

            Assert.IsTrue(home.IsAncestorOrSelf(customDoc4));
            Assert.IsFalse(home.IsAncestorOrSelf(customDoc3));
            Assert.IsTrue(home.IsAncestorOrSelf(customDoc2));
            Assert.IsTrue(home.IsAncestorOrSelf(customDoc));
            Assert.IsTrue(home.IsAncestorOrSelf(home));
            Assert.IsFalse(home.IsAncestorOrSelf(root));

            Assert.IsFalse(customDoc.IsAncestorOrSelf(customDoc4));
            Assert.IsFalse(customDoc.IsAncestorOrSelf(customDoc3));
            Assert.IsTrue(customDoc.IsAncestorOrSelf(customDoc2));
            Assert.IsTrue(customDoc.IsAncestorOrSelf(customDoc));
            Assert.IsFalse(customDoc.IsAncestorOrSelf(home));
            Assert.IsFalse(customDoc.IsAncestorOrSelf(root));

            Assert.IsFalse(customDoc2.IsAncestorOrSelf(customDoc4));
            Assert.IsFalse(customDoc2.IsAncestorOrSelf(customDoc3));
            Assert.IsTrue(customDoc2.IsAncestorOrSelf(customDoc2));
            Assert.IsFalse(customDoc2.IsAncestorOrSelf(customDoc));
            Assert.IsFalse(customDoc2.IsAncestorOrSelf(home));
            Assert.IsFalse(customDoc2.IsAncestorOrSelf(root));

            Assert.IsTrue(customDoc4.IsAncestorOrSelf(customDoc4));
            Assert.IsTrue(customDoc3.IsAncestorOrSelf(customDoc3));
        }


        [Test]
        public void Descendants_Or_Self()
        {
            var doc = GetNode(1046);

            var result = doc.DescendantsOrSelf().ToArray();

            Assert.IsNotNull(result);

            Assert.AreEqual(10, result.Count());
            Assert.IsTrue(result.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1046, 1173, 1174, 1176, 1175 }));
        }

        [Test]
        public void Descendants()
        {
            var doc = GetNode(1046);

            var result = doc.Descendants().ToArray();

            Assert.IsNotNull(result);

            Assert.AreEqual(9, result.Count());
            Assert.IsTrue(result.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1173, 1174, 1176, 1175, 4444 }));
        }

        [Test]
        public void IsDescendant()
        {
            // Structure:
            // - Root : 1046 (no parent)
            // -- Home: 1173 (parent 1046)
            // -- Custom Doc: 1178 (parent 1173)
            // --- Custom Doc2: 1179 (parent: 1178)
            // -- Custom Doc4: 117 (parent 1173)
            // - Custom Doc3: 1172 (no parent)

            var home = GetNode(1173);
            var root = GetNode(1046);
            var customDoc = GetNode(1178);
            var customDoc2 = GetNode(1179);
            var customDoc3 = GetNode(1172);
            var customDoc4 = GetNode(117);

            Assert.IsFalse(root.IsDescendant(root));
            Assert.IsFalse(root.IsDescendant(home));
            Assert.IsFalse(root.IsDescendant(customDoc));
            Assert.IsFalse(root.IsDescendant(customDoc2));
            Assert.IsFalse(root.IsDescendant(customDoc3));
            Assert.IsFalse(root.IsDescendant(customDoc4));

            Assert.IsTrue(home.IsDescendant(root));
            Assert.IsFalse(home.IsDescendant(home));
            Assert.IsFalse(home.IsDescendant(customDoc));
            Assert.IsFalse(home.IsDescendant(customDoc2));
            Assert.IsFalse(home.IsDescendant(customDoc3));
            Assert.IsFalse(home.IsDescendant(customDoc4));

            Assert.IsTrue(customDoc.IsDescendant(root));
            Assert.IsTrue(customDoc.IsDescendant(home));
            Assert.IsFalse(customDoc.IsDescendant(customDoc));
            Assert.IsFalse(customDoc.IsDescendant(customDoc2));
            Assert.IsFalse(customDoc.IsDescendant(customDoc3));
            Assert.IsFalse(customDoc.IsDescendant(customDoc4));

            Assert.IsTrue(customDoc2.IsDescendant(root));
            Assert.IsTrue(customDoc2.IsDescendant(home));
            Assert.IsTrue(customDoc2.IsDescendant(customDoc));
            Assert.IsFalse(customDoc2.IsDescendant(customDoc2));
            Assert.IsFalse(customDoc2.IsDescendant(customDoc3));
            Assert.IsFalse(customDoc2.IsDescendant(customDoc4));

            Assert.IsFalse(customDoc3.IsDescendant(customDoc3));
        }

        [Test]
        public void IsDescendantOrSelf()
        {
            // Structure:
            // - Root : 1046 (no parent)
            // -- Home: 1173 (parent 1046)
            // -- Custom Doc: 1178 (parent 1173)
            // --- Custom Doc2: 1179 (parent: 1178)
            // -- Custom Doc4: 117 (parent 1173)
            // - Custom Doc3: 1172 (no parent)

            var home = GetNode(1173);
            var root = GetNode(1046);
            var customDoc = GetNode(1178);
            var customDoc2 = GetNode(1179);
            var customDoc3 = GetNode(1172);
            var customDoc4 = GetNode(117);

            Assert.IsTrue(root.IsDescendantOrSelf(root));
            Assert.IsFalse(root.IsDescendantOrSelf(home));
            Assert.IsFalse(root.IsDescendantOrSelf(customDoc));
            Assert.IsFalse(root.IsDescendantOrSelf(customDoc2));
            Assert.IsFalse(root.IsDescendantOrSelf(customDoc3));
            Assert.IsFalse(root.IsDescendantOrSelf(customDoc4));

            Assert.IsTrue(home.IsDescendantOrSelf(root));
            Assert.IsTrue(home.IsDescendantOrSelf(home));
            Assert.IsFalse(home.IsDescendantOrSelf(customDoc));
            Assert.IsFalse(home.IsDescendantOrSelf(customDoc2));
            Assert.IsFalse(home.IsDescendantOrSelf(customDoc3));
            Assert.IsFalse(home.IsDescendantOrSelf(customDoc4));

            Assert.IsTrue(customDoc.IsDescendantOrSelf(root));
            Assert.IsTrue(customDoc.IsDescendantOrSelf(home));
            Assert.IsTrue(customDoc.IsDescendantOrSelf(customDoc));
            Assert.IsFalse(customDoc.IsDescendantOrSelf(customDoc2));
            Assert.IsFalse(customDoc.IsDescendantOrSelf(customDoc3));
            Assert.IsFalse(customDoc.IsDescendantOrSelf(customDoc4));

            Assert.IsTrue(customDoc2.IsDescendantOrSelf(root));
            Assert.IsTrue(customDoc2.IsDescendantOrSelf(home));
            Assert.IsTrue(customDoc2.IsDescendantOrSelf(customDoc));
            Assert.IsTrue(customDoc2.IsDescendantOrSelf(customDoc2));
            Assert.IsFalse(customDoc2.IsDescendantOrSelf(customDoc3));
            Assert.IsFalse(customDoc2.IsDescendantOrSelf(customDoc4));

            Assert.IsTrue(customDoc3.IsDescendantOrSelf(customDoc3));
        }

        [Test]
        public void SiblingsAndSelf()
        {
            // Structure:
            // - Root : 1046 (no parent)
            // -- Level1.1: 1173 (parent 1046)
            // --- Level1.1.1: 1174 (parent 1173)
            // --- Level1.1.2: 117 (parent 1173)
            // --- Level1.1.3: 1177 (parent 1173)
            // --- Level1.1.4: 1178 (parent 1173)
            // --- Level1.1.5: 1176 (parent 1173)
            // -- Level1.2: 1175 (parent 1046)
            // -- Level1.3: 4444 (parent 1046)
            var root = GetNode(1046);
            var level1_1 = GetNode(1173);
            var level1_1_1 = GetNode(1174);
            var level1_1_2 = GetNode(117);
            var level1_1_3 = GetNode(1177);
            var level1_1_4 = GetNode(1178);
            var level1_1_5 = GetNode(1176);
            var level1_2 = GetNode(1175);
            var level1_3 = GetNode(4444);

            _publishedSnapshotAccessorMock.Setup(x => x.PublishedSnapshot.Content.GetAtRoot(It.IsAny<string>())).Returns(new []{root});

            CollectionAssertAreEqual(new []{root}, root.SiblingsAndSelf());

            CollectionAssertAreEqual( new []{level1_1, level1_2, level1_3}, level1_1.SiblingsAndSelf());
            CollectionAssertAreEqual( new []{level1_1, level1_2, level1_3}, level1_2.SiblingsAndSelf());
            CollectionAssertAreEqual( new []{level1_1, level1_2, level1_3}, level1_3.SiblingsAndSelf());

            CollectionAssertAreEqual( new []{level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5}, level1_1_1.SiblingsAndSelf());
            CollectionAssertAreEqual( new []{level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5}, level1_1_2.SiblingsAndSelf());
            CollectionAssertAreEqual( new []{level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5}, level1_1_3.SiblingsAndSelf());
            CollectionAssertAreEqual( new []{level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5}, level1_1_4.SiblingsAndSelf());
            CollectionAssertAreEqual( new []{level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5}, level1_1_5.SiblingsAndSelf());

        }

         [Test]
        public void Siblings()
        {
            // Structure:
            // - Root : 1046 (no parent)
            // -- Level1.1: 1173 (parent 1046)
            // --- Level1.1.1: 1174 (parent 1173)
            // --- Level1.1.2: 117 (parent 1173)
            // --- Level1.1.3: 1177 (parent 1173)
            // --- Level1.1.4: 1178 (parent 1173)
            // --- Level1.1.5: 1176 (parent 1173)
            // -- Level1.2: 1175 (parent 1046)
            // -- Level1.3: 4444 (parent 1046)
            var root = GetNode(1046);
            var level1_1 = GetNode(1173);
            var level1_1_1 = GetNode(1174);
            var level1_1_2 = GetNode(117);
            var level1_1_3 = GetNode(1177);
            var level1_1_4 = GetNode(1178);
            var level1_1_5 = GetNode(1176);
            var level1_2 = GetNode(1175);
            var level1_3 = GetNode(4444);

            _publishedSnapshotAccessorMock.Setup(x => x.PublishedSnapshot.Content.GetAtRoot(It.IsAny<string>())).Returns(new []{root});

            CollectionAssertAreEqual(new IPublishedContent[0], root.Siblings());

            CollectionAssertAreEqual( new []{level1_2, level1_3}, level1_1.Siblings());
            CollectionAssertAreEqual( new []{level1_1,  level1_3}, level1_2.Siblings());
            CollectionAssertAreEqual( new []{level1_1, level1_2}, level1_3.Siblings());

            CollectionAssertAreEqual( new []{ level1_1_2, level1_1_3, level1_1_4, level1_1_5}, level1_1_1.Siblings());
            CollectionAssertAreEqual( new []{level1_1_1,  level1_1_3, level1_1_4, level1_1_5}, level1_1_2.Siblings());
            CollectionAssertAreEqual( new []{level1_1_1, level1_1_2,  level1_1_4, level1_1_5}, level1_1_3.Siblings());
            CollectionAssertAreEqual( new []{level1_1_1, level1_1_2, level1_1_3,  level1_1_5}, level1_1_4.Siblings());
            CollectionAssertAreEqual( new []{level1_1_1, level1_1_2, level1_1_3, level1_1_4}, level1_1_5.Siblings());

        }

        private void CollectionAssertAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        where T: IPublishedContent
        {
            var e = expected.Select(x => x.Id);
            var a = actual.Select(x => x.Id);
            CollectionAssert.AreEquivalent(e, a, $"\nExpected:\n{string.Join(", ", e)}\n\nActual:\n{string.Join(", ", a)}");
        }

        [Test]
        public void FragmentProperty()
        {
            var factory = Factory.GetInstance<IPublishedContentTypeFactory>() as PublishedContentTypeFactory;

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
            {
                yield return factory.CreatePropertyType(contentType, "detached", 1003);
            }

            var ct = factory.CreateContentType(Guid.NewGuid(), 0, "alias", CreatePropertyTypes);
            var pt = ct.GetPropertyType("detached");
            var prop = new PublishedElementPropertyBase(pt, null, false, PropertyCacheLevel.None, 5548);
            Assert.IsInstanceOf<int>(prop.GetValue());
            Assert.AreEqual(5548, prop.GetValue());
        }

        public void Fragment1()
        {
            var type = ContentTypesCache.Get(PublishedItemType.Content, "detachedSomething");
            var values = new Dictionary<string, object>();
            var f = new PublishedElement(type, Guid.NewGuid(), values, false);
        }

        [Test]
        public void Fragment2()
        {
            var factory = Factory.GetInstance<IPublishedContentTypeFactory>() as PublishedContentTypeFactory;

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
            {
                yield return factory.CreatePropertyType(contentType, "legend", 1004);
                yield return factory.CreatePropertyType(contentType, "image", 1005);
                yield return factory.CreatePropertyType(contentType, "size", 1003);
            }

            const string val1 = "boom bam";
            const int val2 = 0;
            const int val3 = 666;

            var guid = Guid.NewGuid();

            var ct = factory.CreateContentType(Guid.NewGuid(), 0, "alias", CreatePropertyTypes);

            var c = new ImageWithLegendModel(ct, guid, new Dictionary<string, object>
            {
                { "legend", val1 },
                { "image", val2 },
                { "size", val3 },
            }, false);

            Assert.AreEqual(val1, c.Legend);
            Assert.AreEqual(val3, c.Size);
        }

        class ImageWithLegendModel : PublishedElement
        {
            public ImageWithLegendModel(IPublishedContentType contentType, Guid fragmentKey, Dictionary<string, object> values, bool previewing)
                : base(contentType, fragmentKey, values, previewing)
            { }


            public string Legend => this.Value<string>("legend");

            public IPublishedContent Image => this.Value<IPublishedContent>("image");

            public int Size => this.Value<int>("size");
        }
    }
}

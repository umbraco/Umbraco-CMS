using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Models;

namespace Umbraco.Tests.PublishedContent
{
    /// <summary>
    /// Tests the methods on IPublishedContent using the DefaultPublishedContentStore
    /// </summary>
    [TestFixture]
    public class PublishedContentTests : PublishedContentTestBase
    {
        private PluginManager _pluginManager;
        
        public override void Initialize()
        {
            // required so we can access property.Value
            //PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver();

            base.Initialize();

            // this is so the model factory looks into the test assembly
            _pluginManager = PluginManager.Current;
            PluginManager.Current = new PluginManager(new ActivatorServiceProvider(), CacheHelper.RuntimeCache, ProfilingLogger, false)
            {
                AssembliesToScan = _pluginManager.AssembliesToScan
                    .Union(new[] { typeof(PublishedContentTests).Assembly })
            };

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically
            // when they are requested, but we must declare those that we
            // explicitely want to be here...

            var propertyTypes = new[]
                {
                    // AutoPublishedContentType will auto-generate other properties
                    new PublishedPropertyType("umbracoNaviHide", 0, Constants.PropertyEditors.TrueFalseAlias),
                    new PublishedPropertyType("selectedNodes", 0, "?"),
                    new PublishedPropertyType("umbracoUrlAlias", 0, "?"),
                    new PublishedPropertyType("content", 0, Constants.PropertyEditors.TinyMCEAlias),
                    new PublishedPropertyType("testRecursive", 0, "?"),
                };
            var compositionAliases = new[] { "MyCompositionAlias" };
            var type = new AutoPublishedContentType(0, "anything", "anything", "anything", compositionAliases, propertyTypes);
            PublishedContentType.GetPublishedContentTypeCallback = (alias) => type;
        }

        public override void TearDown()
        {
            PluginManager.Current = _pluginManager;
            ApplicationContext.Current.DisposeIfDisposable();
            ApplicationContext.Current = null;
        }

        protected override void FreezeResolution()
        {
            var types = PluginManager.Current.ResolveTypes<PublishedContentModel>();
            PublishedContentModelFactoryResolver.Current = new PublishedContentModelFactoryResolver(
                new PublishedContentModelFactory(types));
            base.FreezeResolution();
        }

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
		<Home id=""1173"" parentID=""1046"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""Sub1"" urlName=""sub1"" writerName=""admin"" creatorName=""admin"" path=""-1,1046,1173"" isDoc="""">
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
            var ctx = GetUmbracoContext("/test", 1234);
            var doc = ctx.ContentCache.GetById(id);
            Assert.IsNotNull(doc);
            return doc;
        }

        [Test]
        [Ignore("IPublishedContent currently (6.1 as of april 25, 2013) has bugs")]
        public void Fails()
        {
            var content = GetNode(1173);

            var c1 = content.Children.First(x => x.Id == 1177);
            Assert.IsFalse(c1.IsFirst());

            var c2 = content.Children.Where(x => x.DocumentTypeAlias == "CustomDocument").First(x => x.Id == 1177);
            Assert.IsTrue(c2.IsFirst());

            // First is not implemented
            var c2a = content.Children.First(x => x.DocumentTypeAlias == "CustomDocument" && x.Id == 1177);
            Assert.IsTrue(c2a.IsFirst()); // so here it's luck

            c1 = content.Children.First(x => x.Id == 1177);
            Assert.IsFalse(c1.IsFirst()); // and here it fails

            // but even using supported (where) method...
            // do not replace by First(x => ...) here since it's not supported at the moment
            c1 = content.Children.Where(x => x.Id == 1177).First();
            c2 = content.Children.Where(x => x.DocumentTypeAlias == "CustomDocument" && x.Id == 1177).First();

            Assert.IsFalse(c1.IsFirst()); // here it fails because c2 has corrupted it

            // so there's only 1 IPublishedContent instance
            // which keeps changing collection, ie being modified
            // which is *bad* from a cache point of vue
            // and from a consistency point of vue...
            // => we want clones!
        }

        [Test]
        public void Is_Last_From_Where_Filter_Dynamic_Linq()
        {
            var doc = GetNode(1173);

            var items = doc.Children.Where("Visible").ToContentSet();

            foreach (var item in items)
            {
                if (item.Id != 1178)
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
                .Children
                .Where(x => x.IsVisible())
                .ToContentSet();

            Assert.AreEqual(4, items.Count());

            foreach (var d in items)
            {
                switch (d.Id)
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

        [PublishedContentModel("Home")]
        internal class Home : PublishedContentModel
        {
            public Home(IPublishedContent content)
                : base(content)
            { }
        }

        [Test]
        [Ignore("Fails as long as PublishedContentModel is internal.")] // fixme
        public void Is_Last_From_Where_Filter2()
        {
            var doc = GetNode(1173);

            var items = doc.Children
                .Select(x => x.CreateModel()) // linq, returns IEnumerable<IPublishedContent>

                // only way around this is to make sure every IEnumerable<T> extension
                // explicitely returns a PublishedContentSet, not an IEnumerable<T>

                .OfType<Home>() // ours, return IEnumerable<Home> (actually a PublishedContentSet<Home>)
                .Where(x => x.IsVisible()) // so, here it's linq again :-(
                .ToContentSet() // so, we need that one for the test to pass
                .ToArray();

            Assert.AreEqual(1, items.Count());

            foreach (var d in items)
            {
                switch (d.Id)
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

            var items = doc.Children.Take(4).ToContentSet();

            foreach (var item in items)
            {
                if (item.Id != 1178)
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

            foreach (var d in doc.Children.Skip(1))
            {
                if (d.Id != 1176)
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

            var items = doc.Children
                .Concat(new[] { GetNode(1175), GetNode(4444) })
                .ToContentSet();

            foreach (var item in items)
            {
                if (item.Id != 4444)
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
        public void Test_Get_Recursive_Val()
        {
            var doc = GetNode(1174);
            var rVal = doc.GetRecursiveValue("testRecursive");
            var nullVal = doc.GetRecursiveValue("DoNotFindThis");
            Assert.AreEqual("This is the recursive val", rVal);
            Assert.AreEqual("", nullVal);
        }

        [Test]
        public void Get_Property_Value_Uses_Converter()
        {
            var doc = GetNode(1173);

            var propVal = doc.GetPropertyValue("content");
            Assert.IsInstanceOf(typeof(IHtmlString), propVal);
            Assert.AreEqual("<div>This is some content</div>", propVal.ToString());

            var propVal2 = doc.GetPropertyValue<IHtmlString>("content");
            Assert.IsInstanceOf(typeof(IHtmlString), propVal2);
            Assert.AreEqual("<div>This is some content</div>", propVal2.ToString());

            var propVal3 = doc.GetPropertyValue("Content");
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
                .FirstOrDefault(x => x.GetPropertyValue<string>("selectedNodes", "").Split(',').Contains("1173"));

            Assert.IsNotNull(result);
        }

        [Test]
        public void Index()
        {
            var doc = GetNode(1173);
            Assert.AreEqual(0, doc.Index());
            doc = GetNode(1176);
            Assert.AreEqual(4, doc.Index());
            doc = GetNode(1177);
            Assert.AreEqual(2, doc.Index());
            doc = GetNode(1178);
            Assert.AreEqual(3, doc.Index());
        }

        [Test]
        public void Is_First()
        {
            var doc = GetNode(1046); //test root nodes
            Assert.IsTrue(doc.IsFirst());
            doc = GetNode(1172);
            Assert.IsFalse(doc.IsFirst());
            doc = GetNode(1173); //test normal nodes
            Assert.IsTrue(doc.IsFirst());
            doc = GetNode(1175);
            Assert.IsFalse(doc.IsFirst());
        }

        [Test]
        public void Is_Not_First()
        {
            var doc = GetNode(1046); //test root nodes
            Assert.IsFalse(doc.IsNotFirst());
            doc = GetNode(1172);
            Assert.IsTrue(doc.IsNotFirst());
            doc = GetNode(1173); //test normal nodes
            Assert.IsFalse(doc.IsNotFirst());
            doc = GetNode(1175);
            Assert.IsTrue(doc.IsNotFirst());
        }

        [Test]
        public void Is_Position()
        {
            var doc = GetNode(1046); //test root nodes
            Assert.IsTrue(doc.IsPosition(0));
            doc = GetNode(1172);
            Assert.IsTrue(doc.IsPosition(1));
            doc = GetNode(1173); //test normal nodes
            Assert.IsTrue(doc.IsPosition(0));
            doc = GetNode(1175);
            Assert.IsTrue(doc.IsPosition(1));
        }

        [Test]
        public void Children_GroupBy_DocumentTypeAlias()
        {
            var doc = GetNode(1046);

            var found1 = doc.Children.GroupBy("DocumentTypeAlias");

            Assert.AreEqual(2, found1.Count());
            Assert.AreEqual(2, found1.Single(x => x.Key.ToString() == "Home").Count());
            Assert.AreEqual(1, found1.Single(x => x.Key.ToString() == "CustomDocument").Count());
        }

        [Test]
        public void Children_Where_DocumentTypeAlias()
        {
            var doc = GetNode(1046);

            var found1 = doc.Children.Where("DocumentTypeAlias == \"CustomDocument\"");
            var found2 = doc.Children.Where("DocumentTypeAlias == \"Home\"");

            Assert.AreEqual(1, found1.Count());
            Assert.AreEqual(2, found2.Count());
        }

        [Test]
        public void Children_Order_By_Update_Date()
        {
            var doc = GetNode(1173);

            var ordered = doc.Children.OrderBy("UpdateDate");

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
            var model = doc.FirstChild<Home>();

            Assert.IsNotNull(model);
            Assert.IsTrue(model.Id == 1173);
            Assert.IsInstanceOf<Home>(model);
            Assert.IsInstanceOf<IPublishedContent>(model);

            model = doc.FirstChildAs<Home>(x => true); // predicate

            Assert.IsNotNull(model);
            Assert.IsTrue(model.Id == 1173);
            Assert.IsInstanceOf<Home>(model);
            Assert.IsInstanceOf<IPublishedContent>(model);

            doc = GetNode(1175); // does not have child nodes
            Assert.IsNull(doc.FirstChildAs<Home>());
            Assert.IsNull(doc.FirstChildAs<Home>(x => true));
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

            var whereVisible = doc.Ancestors().Where("Visible");
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

            var result = doc.AncestorsOrSelf();

            Assert.IsNotNull(result);

            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1174, 1173, 1046 }));
        }

        [Test]
        public void Ancestors()
        {
            var doc = GetNode(1174);

            var result = doc.Ancestors();

            Assert.IsNotNull(result);

            Assert.AreEqual(2, result.Count());
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

            var result = doc.DescendantsOrSelf();

            Assert.IsNotNull(result);

            Assert.AreEqual(10, result.Count());
            Assert.IsTrue(result.Select(x => ((dynamic)x).Id).ContainsAll(new dynamic[] { 1046, 1173, 1174, 1176, 1175 }));
        }

        [Test]
        public void Descendants()
        {
            var doc = GetNode(1046);

            var result = doc.Descendants();

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
        public void Up()
        {
            var doc = GetNode(1173);

            var result = doc.Up();

            Assert.IsNotNull(result);

            Assert.AreEqual((int)1046, (int)result.Id);
        }

        [Test]
        public void Down()
        {
            var doc = GetNode(1173);

            var result = doc.Down();

            Assert.IsNotNull(result);

            Assert.AreEqual((int)1174, (int)result.Id);
        }

        [Test]
        public void Next()
        {
            var doc = GetNode(1173);

            var result = doc.Next();

            Assert.IsNotNull(result);

            Assert.AreEqual((int)1175, (int)result.Id);
        }

        [Test]
        public void Next_Without_Sibling()
        {
            var doc = GetNode(1176);

            Assert.IsNull(doc.Next());
        }

        [Test]
        public void Previous_Without_Sibling()
        {
            var doc = GetNode(1173);

            Assert.IsNull(doc.Previous());
        }

        [Test]
        public void Previous()
        {
            var doc = GetNode(1176);

            var result = doc.Previous();

            Assert.IsNotNull(result);

            Assert.AreEqual((int)1178, (int)result.Id);
        }

        [Test]
        public void GetKey()
        {
            var key = Guid.Parse("CDB83BBC-A83B-4BA6-93B8-AADEF67D3C09");

            // doc is Home (a model) and GetKey unwraps and works
            var doc = GetNode(1176);
            Assert.IsInstanceOf<Home>(doc);
            Assert.AreEqual(key, doc.GetKey());

            // wrapped is PublishedContentWrapped and WithKey unwraps
            var wrapped = new TestWrapped(doc);
            Assert.AreEqual(key, wrapped.GetKey());
        }

        class TestWrapped : PublishedContentWrapped
        {
            public TestWrapped(IPublishedContent content)
                : base(content)
            { }
        }

        [Test]
        public void DetachedProperty1()
        {
            var type = new PublishedPropertyType("detached", Constants.PropertyEditors.IntegerAlias);
            var prop = PublishedProperty.GetDetached(type.Detached(), "5548");
            Assert.IsInstanceOf<int>(prop.Value);
            Assert.AreEqual(5548, prop.Value);
        }

        public void CreateDetachedContentSample()
        {
            bool previewing = false;
            var t = PublishedContentType.Get(PublishedItemType.Content, "detachedSomething");
            var values = new Dictionary<string, object>();
            var properties = t.PropertyTypes.Select(x =>
            {
                object value;
                if (values.TryGetValue(x.PropertyTypeAlias, out value) == false) value = null;
                return PublishedProperty.GetDetached(x.Detached(), value, previewing);
            });
            // and if you want some sort of "model" it's up to you really...
            var c = new DetachedContent(properties);
        }

        public void CreatedDetachedContentInConverterSample()
        {
            // the converter args
            PublishedPropertyType argPropertyType = null;
            bool argPreview = false;

            var pt1 = new PublishedPropertyType("legend", 0, Constants.PropertyEditors.TextboxAlias);
            var pt2 = new PublishedPropertyType("image", 0, Constants.PropertyEditors.MediaPickerAlias);
            string val1 = "";
            int val2 = 0;

            var c = new ImageWithLegendModel(
                PublishedProperty.GetDetached(pt1.Nested(argPropertyType), val1, argPreview),
                PublishedProperty.GetDetached(pt2.Nested(argPropertyType), val2, argPreview));
        }

        class ImageWithLegendModel
        {
            private IPublishedProperty _legendProperty;
            private IPublishedProperty _imageProperty;

            public ImageWithLegendModel(IPublishedProperty legendProperty, IPublishedProperty imageProperty)
            {
                _legendProperty = legendProperty;
                _imageProperty = imageProperty;
            }

            public string Legend { get { return _legendProperty.GetValue<string>(); } }
            public IPublishedContent Image { get { return _imageProperty.GetValue<IPublishedContent>(); } }
        }
    }
}

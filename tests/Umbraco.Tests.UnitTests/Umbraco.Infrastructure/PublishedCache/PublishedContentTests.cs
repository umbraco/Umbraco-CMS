using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
public class PublishedContentTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var xml = PublishedContentXml.PublishedContentTestXml(1234, _node1173Guid);

        IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
            xml,
            TestHelper.ShortStringHelper,
            out var contentTypes,
            out var dataTypes).ToList();

        _dataTypes = dataTypes;

        // configure the Home content type to be composed of another for tests.
        var compositionType = new ContentType(TestHelper.ShortStringHelper, -1) { Alias = "MyCompositionAlias" };
        contentTypes.First(x => x.Alias == "Home").AddContentType(compositionType);

        InitializedCache(kits, contentTypes, dataTypes);
    }

    private readonly Guid _node1173Guid = Guid.NewGuid();
    private PublishedModelFactory _publishedModelFactory;
    private DataType[] _dataTypes;

    // override to specify our own factory with custom types
    protected override IPublishedModelFactory PublishedModelFactory
        => _publishedModelFactory ??= new PublishedModelFactory(
            new[] { typeof(Home), typeof(Anything), typeof(CustomDocument) },
            PublishedValueFallback);

    [PublishedModel("Home")]
    internal class Home : PublishedContentModel
    {
        public Home(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public bool UmbracoNaviHide => this.Value<bool>(Mock.Of<IPublishedValueFallback>(), "umbracoNaviHide");
    }

    [PublishedModel("anything")]
    internal class Anything : PublishedContentModel
    {
        public Anything(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }
    }

    [PublishedModel("CustomDocument")]
    internal class CustomDocument : PublishedContentModel
    {
        public CustomDocument(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }
    }

    [Test]
    public void GetNodeByIds()
    {
        var snapshot = GetPublishedSnapshot();

        var contentById = snapshot.Content.GetById(1173);
        Assert.IsNotNull(contentById);
        var contentByGuid = snapshot.Content.GetById(_node1173Guid);
        Assert.IsNotNull(contentByGuid);
        Assert.AreEqual(contentById.Id, contentByGuid.Id);
        Assert.AreEqual(contentById.Key, contentByGuid.Key);

        contentById = snapshot.Content.GetById(666);
        Assert.IsNull(contentById);
        contentByGuid = snapshot.Content.GetById(Guid.NewGuid());
        Assert.IsNull(contentByGuid);
    }

    [Test]
    public void Is_Last_From_Where_Filter_Dynamic_Linq()
    {
        var doc = GetContent(1173);

        var items = doc.Children(VariationContextAccessor).Where(x => x.IsVisible(Mock.Of<IPublishedValueFallback>()))
            .ToIndexedArray();

        foreach (var item in items)
        {
            if (item.Content.Id != 1178)
            {
                Assert.IsFalse(item.IsLast(), $"The item {item.Content.Id} is last");
            }
            else
            {
                Assert.IsTrue(item.IsLast(), $"The item {item.Content.Id} is not last");
            }
        }
    }

    [Test]
    public void Is_Last_From_Where_Filter()
    {
        var doc = GetContent(1173);

        var items = doc
            .Children(VariationContextAccessor)
            .Where(x => x.IsVisible(Mock.Of<IPublishedValueFallback>()))
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

    [Test]
    public void Is_Last_From_Where_Filter2()
    {
        var doc = GetContent(1173);
        var ct = doc.ContentType;

        var items = doc.Children(VariationContextAccessor)
            .Select(x => x.CreateModel(PublishedModelFactory)) // linq, returns IEnumerable<IPublishedContent>

            // only way around this is to make sure every IEnumerable<T> extension
            // explicitely returns a PublishedContentSet, not an IEnumerable<T>
            .OfType<Home>() // ours, return IEnumerable<Home> (actually a PublishedContentSet<Home>)
            .Where(x => x.IsVisible(Mock.Of<IPublishedValueFallback>())) // so, here it's linq again :-(
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
        var doc = GetContent(1173);

        var items = doc.Children(VariationContextAccessor).Take(4).ToIndexedArray();

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
        var doc = GetContent(1173);

        foreach (var d in doc.Children(VariationContextAccessor).Skip(1).ToIndexedArray())
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
        var doc = GetContent(1173);

        var items = doc.Children(VariationContextAccessor)
            .Concat(new[] { GetContent(1175), GetContent(4444) })
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
        var doc = GetContent(1046);

        var expected = new[] { 1046, 1173, 1174, 117, 1177, 1178, 1179, 1176, 1175, 4444, 1172 };
        var exindex = 0;

        // must respect the XPath descendants-or-self axis!
        foreach (var d in doc.DescendantsOrSelf(Mock.Of<IVariationContextAccessor>()))
        {
            Assert.AreEqual(expected[exindex++], d.Id);
        }
    }

    [Test]
    public void Get_Property_Value_Recursive()
    {
        // TODO: We need to use a different fallback?
        var doc = GetContent(1174);
        var rVal = doc.Value(PublishedValueFallback, "testRecursive", fallback: Fallback.ToAncestors);
        var nullVal = doc.Value(PublishedValueFallback, "DoNotFindThis", fallback: Fallback.ToAncestors);
        Assert.AreEqual("This is the recursive val", rVal);
        Assert.AreEqual(null, nullVal);
    }

    [Test]
    public void Get_Property_Value_Uses_Converter()
    {
        var doc = GetContent(1173);

        var propVal = doc.Value(PublishedValueFallback, "content");
        Assert.IsInstanceOf(typeof(IHtmlEncodedString), propVal);
        Assert.AreEqual("<div>This is some content</div>", propVal.ToString());

        var propVal2 = doc.Value<IHtmlEncodedString>(PublishedValueFallback, "content");
        Assert.IsInstanceOf(typeof(IHtmlEncodedString), propVal2);
        Assert.AreEqual("<div>This is some content</div>", propVal2.ToString());

        var propVal3 = doc.Value(PublishedValueFallback, "Content");
        Assert.IsInstanceOf(typeof(IHtmlEncodedString), propVal3);
        Assert.AreEqual("<div>This is some content</div>", propVal3.ToString());
    }

    [Test]
    public void Complex_Linq()
    {
        var doc = GetContent(1173);

        var result = doc.Ancestors().OrderBy(x => x.Level)
            .Single()
            .Descendants(Mock.Of<IVariationContextAccessor>())
            .FirstOrDefault(x =>
                x.Value(PublishedValueFallback, "selectedNodes", fallback: Fallback.ToDefaultValue, defaultValue: string.Empty).Split(',').Contains("1173"));

        Assert.IsNotNull(result);
    }

    [Test]
    public void Children_GroupBy_DocumentTypeAlias()
    {
        // var home = new AutoPublishedContentType(Guid.NewGuid(), 22, "Home", new PublishedPropertyType[] { });
        // var custom = new AutoPublishedContentType(Guid.NewGuid(), 23, "CustomDocument", new PublishedPropertyType[] { });
        // var contentTypes = new Dictionary<string, PublishedContentType>
        // {
        //    { home.Alias, home },
        //    { custom.Alias, custom }
        // };
        // ContentTypesCache.GetPublishedContentTypeByAlias = alias => contentTypes[alias];
        var doc = GetContent(1046);

        var found1 = doc.Children(VariationContextAccessor).GroupBy(x => x.ContentType.Alias).ToArray();

        Assert.AreEqual(2, found1.Length);
        Assert.AreEqual(2, found1.Single(x => x.Key.ToString() == "Home").Count());
        Assert.AreEqual(1, found1.Single(x => x.Key.ToString() == "CustomDocument").Count());
    }

    [Test]
    public void Children_Where_DocumentTypeAlias()
    {
        // var home = new AutoPublishedContentType(Guid.NewGuid(), 22, "Home", new PublishedPropertyType[] { });
        // var custom = new AutoPublishedContentType(Guid.NewGuid(), 23, "CustomDocument", new PublishedPropertyType[] { });
        // var contentTypes = new Dictionary<string, PublishedContentType>
        // {
        //     { home.Alias, home },
        //     { custom.Alias, custom }
        // };
        // ContentTypesCache.GetPublishedContentTypeByAlias = alias => contentTypes[alias];
        var doc = GetContent(1046);

        var found1 = doc.Children(VariationContextAccessor).Where(x => x.ContentType.Alias == "CustomDocument");
        var found2 = doc.Children(VariationContextAccessor).Where(x => x.ContentType.Alias == "Home");

        Assert.AreEqual(1, found1.Count());
        Assert.AreEqual(2, found2.Count());
    }

    [Test]
    public void Children_Order_By_Update_Date()
    {
        var doc = GetContent(1173);

        var ordered = doc.Children(VariationContextAccessor).OrderBy(x => x.UpdateDate);

        var correctOrder = new[] { 1178, 1177, 1174, 1176 };
        for (var i = 0; i < correctOrder.Length; i++)
        {
            Assert.AreEqual(correctOrder[i], ordered.ElementAt(i).Id);
        }
    }

    [Test]
    public void FirstChild()
    {
        var doc = GetContent(1173); // has child nodes
        Assert.IsNotNull(doc.FirstChild(Mock.Of<IVariationContextAccessor>()));
        Assert.IsNotNull(doc.FirstChild(Mock.Of<IVariationContextAccessor>(), x => true));
        Assert.IsNotNull(doc.FirstChild<IPublishedContent>(Mock.Of<IVariationContextAccessor>()));

        doc = GetContent(1175); // does not have child nodes
        Assert.IsNull(doc.FirstChild(Mock.Of<IVariationContextAccessor>()));
        Assert.IsNull(doc.FirstChild(Mock.Of<IVariationContextAccessor>(), x => true));
        Assert.IsNull(doc.FirstChild<IPublishedContent>(Mock.Of<IVariationContextAccessor>()));
    }

    [Test]
    public void FirstChildAsT()
    {
        var doc = GetContent(1046); // has child nodes

        var model = doc.FirstChild<Home>(Mock.Of<IVariationContextAccessor>(), x => true); // predicate

        Assert.IsNotNull(model);
        Assert.IsTrue(model.Id == 1173);
        Assert.IsInstanceOf<Home>(model);
        Assert.IsInstanceOf<IPublishedContent>(model);

        doc = GetContent(1175); // does not have child nodes
        Assert.IsNull(doc.FirstChild<Anything>(Mock.Of<IVariationContextAccessor>()));
        Assert.IsNull(doc.FirstChild<Anything>(Mock.Of<IVariationContextAccessor>(), x => true));
    }

    [Test]
    public void IsComposedOf()
    {
        var doc = GetContent(1173);

        var isComposedOf = doc.IsComposedOf("MyCompositionAlias");

        Assert.IsTrue(isComposedOf);
    }

    [Test]
    public void HasProperty()
    {
        var doc = GetContent(1173);

        var hasProp = doc.HasProperty(Constants.Conventions.Content.UrlAlias);

        Assert.IsTrue(hasProp);
    }

    [Test]
    public void HasValue()
    {
        var doc = GetContent(1173);

        var hasValue = doc.HasValue(Mock.Of<IPublishedValueFallback>(), Constants.Conventions.Content.UrlAlias);
        var noValue = doc.HasValue(Mock.Of<IPublishedValueFallback>(), "blahblahblah");

        Assert.IsTrue(hasValue);
        Assert.IsFalse(noValue);
    }

    [Test]
    public void Ancestors_Where_Visible()
    {
        var doc = GetContent(1174);

        var whereVisible = doc.Ancestors().Where(x => x.IsVisible(Mock.Of<IPublishedValueFallback>()));
        Assert.AreEqual(1, whereVisible.Count());
    }

    [Test]
    public void Visible()
    {
        var hidden = GetContent(1046);
        var visible = GetContent(1173);

        Assert.IsFalse(hidden.IsVisible(Mock.Of<IPublishedValueFallback>()));
        Assert.IsTrue(visible.IsVisible(Mock.Of<IPublishedValueFallback>()));
    }

    [Test]
    public void Ancestor_Or_Self()
    {
        var doc = GetContent(1173);

        var result = doc.AncestorOrSelf();

        Assert.IsNotNull(result);

        // ancestor-or-self has to be self!
        Assert.AreEqual(1173, result.Id);
    }

    [Test]
    public void U4_4559()
    {
        var doc = GetContent(1174);
        var result = doc.AncestorOrSelf(1);
        Assert.IsNotNull(result);
        Assert.AreEqual(1046, result.Id);
    }

    [Test]
    public void Ancestors_Or_Self()
    {
        var doc = GetContent(1174);

        var result = doc.AncestorsOrSelf().ToArray();

        Assert.IsNotNull(result);

        Assert.AreEqual(3, result.Length);
        Assert.IsTrue(result.Select(x => x.Id).ContainsAll(new[] { 1174, 1173, 1046 }));
    }

    [Test]
    public void Ancestors()
    {
        var doc = GetContent(1174);

        var result = doc.Ancestors().ToArray();

        Assert.IsNotNull(result);

        Assert.AreEqual(2, result.Length);
        Assert.IsTrue(result.Select(x => x.Id).ContainsAll(new[] { 1173, 1046 }));
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
        var home = GetContent(1173);
        var root = GetContent(1046);
        var customDoc = GetContent(1178);
        var customDoc2 = GetContent(1179);
        var customDoc3 = GetContent(1172);
        var customDoc4 = GetContent(117);

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
        var home = GetContent(1173);
        var root = GetContent(1046);
        var customDoc = GetContent(1178);
        var customDoc2 = GetContent(1179);
        var customDoc3 = GetContent(1172);
        var customDoc4 = GetContent(117);

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
        var doc = GetContent(1046);

        var result = doc.DescendantsOrSelf(Mock.Of<IVariationContextAccessor>()).ToArray();

        Assert.IsNotNull(result);

        Assert.AreEqual(10, result.Count());
        Assert.IsTrue(result.Select(x => x.Id).ContainsAll(new[] { 1046, 1173, 1174, 1176, 1175 }));
    }

    [Test]
    public void Descendants()
    {
        var doc = GetContent(1046);

        var result = doc.Descendants(Mock.Of<IVariationContextAccessor>()).ToArray();

        Assert.IsNotNull(result);

        Assert.AreEqual(9, result.Count());
        Assert.IsTrue(result.Select(x => x.Id).ContainsAll(new[] { 1173, 1174, 1176, 1175, 4444 }));
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
        var home = GetContent(1173);
        var root = GetContent(1046);
        var customDoc = GetContent(1178);
        var customDoc2 = GetContent(1179);
        var customDoc3 = GetContent(1172);
        var customDoc4 = GetContent(117);

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
        var home = GetContent(1173);
        var root = GetContent(1046);
        var customDoc = GetContent(1178);
        var customDoc2 = GetContent(1179);
        var customDoc3 = GetContent(1172);
        var customDoc4 = GetContent(117);

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
        // ---- Level1.1.4.1: 1179 (parent 1178)
        // --- Level1.1.5: 1176 (parent 1173)
        // -- Level1.2: 1175 (parent 1046)
        // -- Level1.3: 4444 (parent 1046)
        // - Root : 1172 (no parent)
        var root = GetContent(1046);
        var level1_1 = GetContent(1173);
        var level1_1_1 = GetContent(1174);
        var level1_1_2 = GetContent(117);
        var level1_1_3 = GetContent(1177);
        var level1_1_4 = GetContent(1178);
        var level1_1_5 = GetContent(1176);
        var level1_2 = GetContent(1175);
        var level1_3 = GetContent(4444);
        var root2 = GetContent(1172);

        var publishedSnapshot = GetPublishedSnapshot();

        CollectionAssertAreEqual(new[] { root, root2 }, root.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));

        CollectionAssertAreEqual(new[] { level1_1, level1_2, level1_3 }, level1_1.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1, level1_2, level1_3 }, level1_2.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1, level1_2, level1_3 }, level1_3.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));

        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5 }, level1_1_1.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5 }, level1_1_2.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5 }, level1_1_3.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5 }, level1_1_4.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_2, level1_1_3, level1_1_4, level1_1_5 }, level1_1_5.SiblingsAndSelf(publishedSnapshot, VariationContextAccessor));
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
        // ---- Level1.1.4.1: 1179 (parent 1178)
        // --- Level1.1.5: 1176 (parent 1173)
        // -- Level1.2: 1175 (parent 1046)
        // -- Level1.3: 4444 (parent 1046)
        // - Root : 1172 (no parent)
        var root = GetContent(1046);
        var level1_1 = GetContent(1173);
        var level1_1_1 = GetContent(1174);
        var level1_1_2 = GetContent(117);
        var level1_1_3 = GetContent(1177);
        var level1_1_4 = GetContent(1178);
        var level1_1_5 = GetContent(1176);
        var level1_2 = GetContent(1175);
        var level1_3 = GetContent(4444);
        var root2 = GetContent(1172);

        var publishedSnapshot = GetPublishedSnapshot();

        CollectionAssertAreEqual(new[] { root2 }, root.Siblings(publishedSnapshot, VariationContextAccessor));

        CollectionAssertAreEqual(new[] { level1_2, level1_3 }, level1_1.Siblings(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1, level1_3 }, level1_2.Siblings(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1, level1_2 }, level1_3.Siblings(publishedSnapshot, VariationContextAccessor));

        CollectionAssertAreEqual(new[] { level1_1_2, level1_1_3, level1_1_4, level1_1_5 }, level1_1_1.Siblings(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_3, level1_1_4, level1_1_5 }, level1_1_2.Siblings(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_2, level1_1_4, level1_1_5 }, level1_1_3.Siblings(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_2, level1_1_3, level1_1_5 }, level1_1_4.Siblings(publishedSnapshot, VariationContextAccessor));
        CollectionAssertAreEqual(new[] { level1_1_1, level1_1_2, level1_1_3, level1_1_4 }, level1_1_5.Siblings(publishedSnapshot, VariationContextAccessor));
    }

    private void CollectionAssertAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        where T : IPublishedContent
    {
        var e = expected.Select(x => x.Id).ToArray();
        var a = actual.Select(x => x.Id).ToArray();
        CollectionAssert.AreEquivalent(e, a, $"\nExpected:\n{string.Join(", ", e)}\n\nActual:\n{string.Join(", ", a)}");
    }

    [Test]
    public void FragmentProperty()
    {
        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            yield return PublishedContentTypeFactory.CreatePropertyType(contentType, "detached", _dataTypes[0].Id);
        }

        var ct = PublishedContentTypeFactory.CreateContentType(Guid.NewGuid(), 0, "alias", CreatePropertyTypes);
        var pt = ct.GetPropertyType("detached");
        var prop = new PublishedElementPropertyBase(pt, null, false, PropertyCacheLevel.None, 5548);
        Assert.IsInstanceOf<int>(prop.GetValue());
        Assert.AreEqual(5548, prop.GetValue());
    }

    [Test]
    public void Fragment2()
    {
        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            yield return PublishedContentTypeFactory.CreatePropertyType(contentType, "legend", _dataTypes[0].Id);
            yield return PublishedContentTypeFactory.CreatePropertyType(contentType, "image", _dataTypes[0].Id);
            yield return PublishedContentTypeFactory.CreatePropertyType(contentType, "size", _dataTypes[0].Id);
        }

        const string val1 = "boom bam";
        const int val2 = 0;
        const int val3 = 666;

        var guid = Guid.NewGuid();

        var ct = PublishedContentTypeFactory.CreateContentType(Guid.NewGuid(), 0, "alias", CreatePropertyTypes);

        var c = new ImageWithLegendModel(
            ct,
            guid,
            new Dictionary<string, object> { { "legend", val1 }, { "image", val2 }, { "size", val3 } },
            false);

        Assert.AreEqual(val1, c.Legend);
        Assert.AreEqual(val3, c.Size);
    }

    [Test]
    public void First()
    {
        var publishedSnapshot = GetPublishedSnapshot();
        var content = publishedSnapshot.Content.GetAtRoot().First();
        Assert.AreEqual("Home", content.Name(VariationContextAccessor));
    }

    [Test]
    public void Distinct()
    {
        var items = GetContent(1173)
            .Children(VariationContextAccessor)
            .Distinct()
            .Distinct()
            .ToIndexedArray();

        Assert.AreEqual(5, items.Length);

        var item = items[0];
        Assert.AreEqual(1174, item.Content.Id);
        Assert.IsTrue(item.IsFirst());
        Assert.IsFalse(item.IsLast());

        item = items[^1];
        Assert.AreEqual(1176, item.Content.Id);
        Assert.IsFalse(item.IsFirst());
        Assert.IsTrue(item.IsLast());
    }

    [Test]
    public void OfType1()
    {
        var publishedSnapshot = GetPublishedSnapshot();
        var items = publishedSnapshot.Content.GetAtRoot()
            .OfType<Home>()
            .Distinct()
            .ToIndexedArray();
        Assert.AreEqual(1, items.Length);
        Assert.IsInstanceOf<Home>(items.First().Content);
    }

    [Test]
    public void OfType2()
    {
        var publishedSnapshot = GetPublishedSnapshot();
        var content = publishedSnapshot.Content.GetAtRoot()
            .OfType<CustomDocument>()
            .Distinct()
            .ToIndexedArray();
        Assert.AreEqual(1, content.Length);
        Assert.IsInstanceOf<CustomDocument>(content.First().Content);
    }

    [Test]
    public void OfType()
    {
        var content = GetContent(1173)
            .Children(VariationContextAccessor)
            .OfType<Home>()
            .First(x => x.UmbracoNaviHide);
        Assert.AreEqual(1176, content.Id);
    }

    [Test]
    public void Position()
    {
        var items = GetContent(1173).Children(VariationContextAccessor)
            .Where(x => x.Value<int?>(Mock.Of<IPublishedValueFallback>(), "umbracoNaviHide") == 0)
            .ToIndexedArray();

        Assert.AreEqual(3, items.Length);

        Assert.IsTrue(items.First().IsFirst());
        Assert.IsFalse(items.First().IsLast());
        Assert.IsFalse(items.Skip(1).First().IsFirst());
        Assert.IsFalse(items.Skip(1).First().IsLast());
        Assert.IsFalse(items.Skip(2).First().IsFirst());
        Assert.IsTrue(items.Skip(2).First().IsLast());
    }

    private class ImageWithLegendModel : PublishedElement
    {
        public ImageWithLegendModel(
            IPublishedContentType contentType,
            Guid fragmentKey,
            Dictionary<string, object> values,
            bool previewing)
            : base(contentType, fragmentKey, values, previewing)
        {
        }

        public string Legend => this.Value<string>(Mock.Of<IPublishedValueFallback>(), "legend");

        public IPublishedContent Image => this.Value<IPublishedContent>(Mock.Of<IPublishedValueFallback>(), "image");

        public int Size => this.Value<int>(Mock.Of<IPublishedValueFallback>(), "size");
    }

    // [PublishedModel("ContentType2")]
    // public class ContentType2 : PublishedContentModel
    // {
    //     #region Plumbing

    // public ContentType2(IPublishedContent content, IPublishedValueFallback fallback)
    // : base(content, fallback)
    // { }

    // #endregion

    // public int Prop1 => this.Value<int>(Mock.Of<IPublishedValueFallback>(), "prop1");
    // }

    // [PublishedModel("ContentType2Sub")]
    // public class ContentType2Sub : ContentType2
    // {
    //     #region Plumbing

    // public ContentType2Sub(IPublishedContent content, IPublishedValueFallback fallback)
    // : base(content, fallback)
    // { }

    // #endregion
    // }
}

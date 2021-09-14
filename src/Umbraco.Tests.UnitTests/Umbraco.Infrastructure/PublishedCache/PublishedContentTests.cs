using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache
{
    [TestFixture]
    public class PublishedContentTests : PublishedSnapshotServiceTestBase
    {
        private readonly Guid _node1173Guid = Guid.NewGuid();
        private PublishedModelFactory _publishedModelFactory;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            string xml = PublishedContentXml.GetXmlContent3(1234, _node1173Guid);

            IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
                xml,
                TestHelper.ShortStringHelper,
                out ContentType[] contentTypes,
                out DataType[] dataTypes).ToList();

            Init(kits, contentTypes, dataTypes);
        }

        // override to specify our own factory with custom types
        protected override IPublishedModelFactory PublishedModelFactory
            => _publishedModelFactory ??= new PublishedModelFactory(
                    new[] { typeof(Home), typeof(Anything) },
                    PublishedValueFallback);

        [PublishedModel("Home")]
        internal class Home : PublishedContentModel
        {
            public Home(IPublishedContent content, IPublishedValueFallback fallback)
                : base(content, fallback)
            { }
        }

        [PublishedModel("anything")]
        internal class Anything : PublishedContentModel
        {
            public Anything(IPublishedContent content, IPublishedValueFallback fallback)
                : base(content, fallback)
            { }
        }

        internal IPublishedContent GetNode(int id)
        {
            var snapshot = GetPublishedSnapshot();
            var doc = snapshot.Content.GetById(id);
            Assert.IsNotNull(doc);
            return doc;
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
            var doc = GetNode(1173);

            var items = doc.Children(VariationContextAccessor).Where(x => x.IsVisible(Mock.Of<IPublishedValueFallback>())).ToIndexedArray();

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
            var doc = GetNode(1173);

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
            var doc = GetNode(1173);
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
            var doc = GetNode(1173);

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
            var doc = GetNode(1173);

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
            var doc = GetNode(1173);

            var items = doc.Children(VariationContextAccessor)
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
            foreach (var d in doc.DescendantsOrSelf(Mock.Of<IVariationContextAccessor>()))
            {
                Assert.AreEqual(expected[exindex++], d.Id);
            }
        }

        [Test]
        public void Get_Property_Value_Recursive()
        {
            // TODO: We need to use a different fallback?

            var doc = GetNode(1174);
            var rVal = doc.Value(PublishedValueFallback, "testRecursive", fallback: Fallback.ToAncestors);
            var nullVal = doc.Value(PublishedValueFallback, "DoNotFindThis", fallback: Fallback.ToAncestors);
            Assert.AreEqual("This is the recursive val", rVal);
            Assert.AreEqual(null, nullVal);
        }

        [Test]
        public void Get_Property_Value_Uses_Converter()
        {
            var doc = GetNode(1173);

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
            var doc = GetNode(1173);

            var result = doc.Ancestors().OrderBy(x => x.Level)
                .Single()
                .Descendants(Mock.Of<IVariationContextAccessor>())
                .FirstOrDefault(x => x.Value<string>(PublishedValueFallback, "selectedNodes", defaultValue: "").Split(',').Contains("1173"));

            Assert.IsNotNull(result);
        }
    }
}

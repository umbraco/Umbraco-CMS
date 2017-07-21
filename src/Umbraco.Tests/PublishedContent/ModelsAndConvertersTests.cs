using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class ModelsAndConvertersTests
    {
        [Test]
        public void ModelTypeEqualityTests()
        {
            Assert.AreNotEqual(ModelType.For("alias1"), ModelType.For("alias1"));

            Assert.IsTrue(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias1")));
            Assert.IsFalse(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias2")));

            Assert.IsTrue(ModelType.Equals(typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias1"))));
            Assert.IsFalse(ModelType.Equals(typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias2"))));

            Assert.IsTrue(ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias1").MakeArrayType()));
            Assert.IsFalse(ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias2").MakeArrayType()));
        }

        [Test]
        public void ModelTypeToStringTests()
        {
            Assert.AreEqual("{alias1}", ModelType.For("alias1").ToString());

            // there's an "*" there because the arrays are not true SZArray - but that changes when we map
            Assert.AreEqual("{alias1}[*]", ModelType.For("alias1").MakeArrayType().ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[{alias1}[*]]", typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()).ToString());
        }

        [Test]
        public void ModelTypeMapTests()
        {
            var map = new Dictionary<string, Type>
            {
                { "alias1", typeof (TestSetModel1) },
                { "alias2", typeof (TestSetModel2) },
            };

            Assert.AreEqual("Umbraco.Tests.PublishedContent.ModelsAndConvertersTests+TestSetModel1",
                ModelType.Map(ModelType.For("alias1"), map).ToString());
            Assert.AreEqual("Umbraco.Tests.PublishedContent.ModelsAndConvertersTests+TestSetModel1[]",
                ModelType.Map(ModelType.For("alias1").MakeArrayType(), map).ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[Umbraco.Tests.PublishedContent.ModelsAndConvertersTests+TestSetModel1]",
                ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), map).ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[Umbraco.Tests.PublishedContent.ModelsAndConvertersTests+TestSetModel1[]]",
                ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()), map).ToString());
        }

        [Test]
        public void ConverterTest1()
        {
            Current.Reset();
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            Current.Container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append<TestConverter1>()
                .Append<TestConverter2>();

            IPublishedContentModelFactory factory = new PublishedContentModelFactory(new[]
            {
                typeof(TestSetModel1), typeof(TestSetModel2),
                typeof(TestContentModel1), typeof(TestContentModel2),
            });
            Current.Container.Register(f => factory);

            var setType1 = new PublishedContentType(1000, "set1", new[]
            {
                new PublishedPropertyType("prop1", "editor1"),
            });

            var setType2 = new PublishedContentType(1001, "set2", new[]
            {
                new PublishedPropertyType("prop2", "editor2"),
            });

            var contentType1 = new PublishedContentType(1002, "content1", new[]
            {
                new PublishedPropertyType("prop1", "editor1"),
            });

            var contentType2 = new PublishedContentType(1003, "content2", new[]
            {
                new PublishedPropertyType("prop2", "editor2"),
            });

            var set1 = new PropertySet(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var set2 = new PropertySet(setType2, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);
            var cnt1 = new TestPublishedContent(contentType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "val1" } }, false);
            var cnt2 = new TestPublishedContent(contentType2, Guid.NewGuid(), new Dictionary<string, object> { { "prop2", "1003" } }, false);

            var cache = new Dictionary<int, IPublishedContent>
            {
                { 1003, cnt1.CreateModel() },
                { 1004, cnt2.CreateModel() },
            };

            var facadeMock = new Mock<IFacade>();
            var cacheMock = new Mock<IPublishedContentCache>();
            cacheMock.Setup(x => x.GetById(It.IsAny<int>())).Returns<int>(id => cache.TryGetValue(id, out IPublishedContent content) ? content : null);
            facadeMock.Setup(x => x.ContentCache).Returns(cacheMock.Object);
            var facade = facadeMock.Object;
            Current.Container.Register(f => facade);

            // can get the actual property Clr type
            // ie ModelType gets properly mapped by IPublishedContentModelFactory
            // must test ModelClrType with special equals 'cos they are not ref-equals
            Assert.IsTrue(ModelType.Equals(typeof (IEnumerable<>).MakeGenericType(ModelType.For("content1")), contentType2.GetPropertyType("prop2").ModelClrType));
            Assert.AreEqual(typeof (IEnumerable<TestContentModel1>), contentType2.GetPropertyType("prop2").ClrType);

            // can create a model for a property set
            var model1 = factory.CreateModel(set1);
            Assert.IsInstanceOf<TestSetModel1>(model1);
            Assert.AreEqual("val1", ((TestSetModel1) model1).Prop1);

            // can create a model for a published content
            var model2 = factory.CreateModel(set2);
            Assert.IsInstanceOf<TestSetModel2>(model2);
            var mmodel2 = (TestSetModel2) model2;

            // and get direct property
            Assert.IsInstanceOf<TestContentModel1[]>(model2.Value("prop2"));
            Assert.AreEqual(1, ((TestContentModel1[]) model2.Value("prop2")).Length);

            // and get model property
            Assert.IsInstanceOf<IEnumerable<TestContentModel1>>(mmodel2.Prop2);
            Assert.IsInstanceOf<TestContentModel1[]>(mmodel2.Prop2);
            var mmodel1 = mmodel2.Prop2.First();

            // and we get what we want
            Assert.AreSame(cache[1003], mmodel1);
        }

        internal class TestPublishedContent : PropertySet, IPublishedContent
        {
            public TestPublishedContent(PublishedContentType contentType, Guid key, Dictionary<string, object> values, bool previewing)
                : base(contentType, key, values, previewing)
            { }

            public int Id { get; }
            public int TemplateId { get; }
            public int SortOrder { get; }
            public string Name { get; }
            public string UrlName { get; }
            public string DocumentTypeAlias { get; }
            public int DocumentTypeId { get; }
            public string WriterName { get; }
            public string CreatorName { get; }
            public int WriterId { get; }
            public int CreatorId { get; }
            public string Path { get; }
            public DateTime CreateDate { get; }
            public DateTime UpdateDate { get; }
            public Guid Version { get; }
            public int Level { get; }
            public string Url { get; }
            public PublishedItemType ItemType { get; }
            public bool IsDraft { get; }
            public IPublishedContent Parent { get; }
            public IEnumerable<IPublishedContent> Children { get; }
            public IPublishedProperty GetProperty(string alias, bool recurse)
            {
                throw new NotImplementedException();
            }
        }

        [PublishedContentModel("set1")]
        public class TestSetModel1 : PropertySetModel
        {
            public TestSetModel1(IPropertySet content)
                : base(content)
            { }

            public string Prop1 => this.Value<string>("prop1");
        }

        [PublishedContentModel("set2")]
        public class TestSetModel2 : PropertySetModel
        {
            public TestSetModel2(IPropertySet content)
                : base(content)
            { }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>("prop2");
        }

        [PublishedContentModel("content1")]
        public class TestContentModel1 : PublishedContentModel
        {
            public TestContentModel1(IPublishedContent content)
                : base(content)
            { }

            public string Prop1 => this.Value<string>("prop1");
        }

        [PublishedContentModel("content2")]
        public class TestContentModel2 : PublishedContentModel
        {
            public TestContentModel2(IPublishedContent content)
                : base(content)
            { }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>("prop2");
        }

        public class TestConverter1 : PropertyValueConverterBase
        {
            public override bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.PropertyEditorAlias == "editor1";

            public override Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (string);

            public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Content;
        }

        public class TestConverter2 : PropertyValueConverterBase
        {
            private readonly IFacade _facade;

            public TestConverter2(IFacade facade)
            {
                _facade = facade;
            }

            public override bool IsConverter(PublishedPropertyType propertyType)
                => propertyType.PropertyEditorAlias == "editor2";

            // pretend ... when writing the converter, the model type for alias "set1" does not exist yet
            public override Type GetPropertyValueType(PublishedPropertyType propertyType)
                => typeof (IEnumerable<>).MakeGenericType(ModelType.For("content1"));

            public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
                => PropertyCacheLevel.Snapshot;

            public override object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
            {
                var s = source as string;
                return s?.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();
            }

            public override object ConvertInterToObject(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
            {
                return ((int[]) inter).Select(x => (TestContentModel1) _facade.ContentCache.GetById(x)).ToArray();
            }
        }
    }
}

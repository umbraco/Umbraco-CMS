using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Core;
using Umbraco.Tests.Testing;
using Umbraco.Web.Composing;
using System;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    [UmbracoTest(TypeLoader = UmbracoTestOptions.TypeLoader.PerFixture)]
    public class PublishedContentMoreTests : PublishedContentSnapshotTestBase
    {
        internal override void PopulateCache(PublishedContentTypeFactory factory, SolidPublishedContentCache cache)
        {
            IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
            {
                yield return factory.CreatePropertyType(contentType, "prop1", 1);
            }

            var contentType1 = factory.CreateContentType(Guid.NewGuid(), 1, "ContentType1", Enumerable.Empty<string>(), CreatePropertyTypes);
            var contentType2 = factory.CreateContentType(Guid.NewGuid(), 2, "ContentType2", Enumerable.Empty<string>(), CreatePropertyTypes);
            var contentType2Sub = factory.CreateContentType(Guid.NewGuid(), 3, "ContentType2Sub", Enumerable.Empty<string>(), CreatePropertyTypes);

            var content = new SolidPublishedContent(contentType1)
            {
                Id = 1,
                SortOrder = 0,
                Name = "Content 1",
                UrlSegment = "content-1",
                Path = "/1",
                Level = 1,
                Url = "/content-1",
                ParentId = -1,
                ChildIds = new int[] { },
                Properties = new Collection<IPublishedProperty>
                {
                    new SolidPublishedProperty
                    {
                        Alias = "prop1",
                        SolidHasValue = true,
                        SolidValue = 1234,
                        SolidSourceValue = "1234"
                    }
                }
            };
            cache.Add(content);

            content = new SolidPublishedContent(contentType2)
            {
                Id = 2,
                SortOrder = 1,
                Name = "Content 2",
                UrlSegment = "content-2",
                Path = "/2",
                Level = 1,
                Url = "/content-2",
                ParentId = -1,
                ChildIds = new int[] { },
                Properties = new Collection<IPublishedProperty>
                {
                    new SolidPublishedProperty
                    {
                        Alias = "prop1",
                        SolidHasValue = true,
                        SolidValue = 1234,
                        SolidSourceValue = "1234"
                    }
                }
            };
            cache.Add(content);

            content = new SolidPublishedContent(contentType2Sub)
            {
                Id = 3,
                SortOrder = 2,
                Name = "Content 2Sub",
                UrlSegment = "content-2sub",
                Path = "/3",
                Level = 1,
                Url = "/content-2sub",
                ParentId = -1,
                ChildIds = new int[] { },
                Properties = new Collection<IPublishedProperty>
                {
                    new SolidPublishedProperty
                    {
                        Alias = "prop1",
                        SolidHasValue = true,
                        SolidValue = 1234,
                        SolidSourceValue = "1234"
                    }
                }
            };
            cache.Add(content);
        }

        [Test]
        public void First()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot().First();
            Assert.AreEqual("Content 1", content.Name());
        }

        [Test]
        public void Distinct()
        {
            var items = Current.UmbracoContext.Content.GetAtRoot()
                .Distinct()
                .Distinct()
                .ToIndexedArray();

            var item = items[0];
            Assert.AreEqual("Content 1", item.Content.Name);
            Assert.IsTrue(item.IsFirst());
            Assert.IsFalse(item.IsLast());

            item = items[1];
            Assert.AreEqual("Content 2", item.Content.Name);
            Assert.IsFalse(item.IsFirst());
            Assert.IsFalse(item.IsLast());

            item = items[2];
            Assert.AreEqual("Content 2Sub", item.Content.Name);
            Assert.IsFalse(item.IsFirst());
            Assert.IsTrue(item.IsLast());
        }

        [Test]
        public void OfType1()
        {
            var items = Current.UmbracoContext.Content.GetAtRoot()
                .OfType<ContentType2>()
                .Distinct()
                .ToIndexedArray();
            Assert.AreEqual(2, items.Length);
            Assert.IsInstanceOf<ContentType2>(items.First().Content);
        }

        [Test]
        public void OfType2()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot()
                .OfType<ContentType2Sub>()
                .Distinct()
                .ToIndexedArray();
            Assert.AreEqual(1, content.Length);
            Assert.IsInstanceOf<ContentType2Sub>(content.First().Content);
        }

        [Test]
        public void OfType()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot()
                .OfType<ContentType2>()
                .First(x => x.Prop1 == 1234);
            Assert.AreEqual("Content 2", content.Name);
            Assert.AreEqual(1234, content.Prop1);
        }

        [Test]
        public void Position()
        {
            var items = Current.UmbracoContext.Content.GetAtRoot()
                .Where(x => x.Value<int>("prop1") == 1234)
                .ToIndexedArray();

            Assert.IsTrue(items.First().IsFirst());
            Assert.IsFalse(items.First().IsLast());
            Assert.IsFalse(items.Skip(1).First().IsFirst());
            Assert.IsFalse(items.Skip(1).First().IsLast());
            Assert.IsFalse(items.Skip(2).First().IsFirst());
            Assert.IsTrue(items.Skip(2).First().IsLast());
        }

        [Test]
        public void Issue()
        {
            var content = Current.UmbracoContext.Content.GetAtRoot()
                .Distinct()
                .OfType<ContentType2>();

            var where = content.Where(x => x.Prop1 == 1234);
            var first = where.First();
            Assert.AreEqual(1234, first.Prop1);

            var content2 = Current.UmbracoContext.Content.GetAtRoot()
                .OfType<ContentType2>()
                .First(x => x.Prop1 == 1234);
            Assert.AreEqual(1234, content2.Prop1);

            var content3 = Current.UmbracoContext.Content.GetAtRoot()
                .OfType<ContentType2>()
                .First();
            Assert.AreEqual(1234, content3.Prop1);
        }

        [Test]
        public void PublishedContentQueryTypedContentList()
        {
            var query = new PublishedContentQuery(Current.UmbracoContext.PublishedSnapshot, Current.UmbracoContext.VariationContextAccessor);
            var result = query.Content(new[] { 1, 2, 4 }).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
        }
    }
}

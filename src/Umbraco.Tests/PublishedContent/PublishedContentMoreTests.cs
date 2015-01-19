using System.Linq;
using System.Collections.ObjectModel;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Tests.TestHelpers;
using umbraco.BusinessLogic;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Security;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class PublishedContentMoreTests : PublishedContentTestBase
    {

        // read http://stackoverflow.com/questions/7713326/extension-method-that-works-on-ienumerablet-and-iqueryablet
        // and http://msmvps.com/blogs/jon_skeet/archive/2010/10/28/overloading-and-generic-constraints.aspx
        // and http://blogs.msdn.com/b/ericlippert/archive/2009/12/10/constraints-are-not-part-of-the-signature.aspx

        private PluginManager _pluginManager;

        public override void Initialize()
        {
            base.Initialize();

            // this is so the model factory looks into the test assembly
            _pluginManager = PluginManager.Current;
            PluginManager.Current = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(), ProfilingLogger, false)
                {
                    AssembliesToScan = _pluginManager.AssembliesToScan
                        .Union(new[] { typeof (PublishedContentMoreTests).Assembly})
                };

            InitializeUmbracoContext();
        }

        protected override void FreezeResolution()
        {
            PropertyValueConvertersResolver.Current =
                new PropertyValueConvertersResolver(new ActivatorServiceProvider(), Logger);
            var types = PluginManager.Current.ResolveTypes<PublishedContentModel>();
            PublishedContentModelFactoryResolver.Current =
                new PublishedContentModelFactoryResolver(new PublishedContentModelFactory(types));

            base.FreezeResolution();
        }

        private void InitializeUmbracoContext()
        {
            RouteData routeData = null;

            var caches = CreatePublishedContent();

            var httpContext = GetHttpContextFactory("http://umbraco.local/", routeData).HttpContext;
            var ctx = new UmbracoContext(
                httpContext,
                ApplicationContext,
                caches,
                new WebSecurity(httpContext, ApplicationContext));

            UmbracoContext.Current = ctx;
        }
        
        public override void TearDown()
        {
            PluginManager.Current = _pluginManager;
            ApplicationContext.Current.DisposeIfDisposable();
            ApplicationContext.Current = null;
        }

        [Test]
        public void First()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            Assert.AreEqual("Content 1", content.Name);
        }

        [Test]
        public void DefaultContentSetIsSiblings()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            Assert.AreEqual(0, content.Index());
            Assert.IsTrue(content.IsFirst());
        }

        [Test]
        public void RunOnLatestContentSet()
        {
            // get first content
            var content = UmbracoContext.Current.ContentCache.GetAtRoot().First();
            var id = content.Id;
            Assert.IsTrue(content.IsFirst());

            // reverse => should be last, but set has not changed => still first
            content = UmbracoContext.Current.ContentCache.GetAtRoot().Reverse().First(x => x.Id == id);
            Assert.IsTrue(content.IsFirst());
            Assert.IsFalse(content.IsLast());

            // reverse + new set => now it's last
            content = UmbracoContext.Current.ContentCache.GetAtRoot().Reverse().ToContentSet().First(x => x.Id == id);
            Assert.IsFalse(content.IsFirst());
            Assert.IsTrue(content.IsLast());

            // reverse that set => should be first, but no new set => still last
            content = UmbracoContext.Current.ContentCache.GetAtRoot().Reverse().ToContentSet().Reverse().First(x => x.Id == id);
            Assert.IsFalse(content.IsFirst());
            Assert.IsTrue(content.IsLast());
        }

        [Test]
        public void Distinct()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot()
                .Distinct()
                .Distinct()
                .ToContentSet()
                .First();

            Assert.AreEqual("Content 1", content.Name);
            Assert.IsTrue(content.IsFirst());
            Assert.IsFalse(content.IsLast());

            content = content.Next();
            Assert.AreEqual("Content 2", content.Name);
            Assert.IsFalse(content.IsFirst());
            Assert.IsFalse(content.IsLast());

            content = content.Next();
            Assert.AreEqual("Content 2Sub", content.Name);
            Assert.IsFalse(content.IsFirst());
            Assert.IsTrue(content.IsLast());
        }

        [Test]
        [Ignore("Fails as long as PublishedContentModel is internal.")] // fixme
        public void OfType1()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot()
                .OfType<ContentType2>()
                .Distinct()
                .ToArray();
            Assert.AreEqual(2, content.Count());
            Assert.IsInstanceOf<ContentType2>(content.First());
            var set = content.ToContentSet();
            Assert.IsInstanceOf<ContentType2>(set.First());
            Assert.AreSame(set, set.First().ContentSet);
            Assert.IsInstanceOf<ContentType2Sub>(set.First().Next());
        }

        [Test]
        [Ignore("Fails as long as PublishedContentModel is internal.")] // fixme
        public void OfType2()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot()
                .OfType<ContentType2Sub>()
                .Distinct()
                .ToArray();
            Assert.AreEqual(1, content.Count());
            Assert.IsInstanceOf<ContentType2Sub>(content.First());
            var set = content.ToContentSet();
            Assert.IsInstanceOf<ContentType2Sub>(set.First());
        }

        [Test]
        [Ignore("Fails as long as PublishedContentModel is internal.")] // fixme
        public void OfType()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot()
                .OfType<ContentType2>()
                .First(x => x.Prop1 == 1234);
            Assert.AreEqual("Content 2", content.Name);
            Assert.AreEqual(1234, content.Prop1);
        }

        [Test]
        public void Position()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot()
                .Where(x => x.GetPropertyValue<int>("prop1") == 1234)
                .ToContentSet()
                .ToArray();

            Assert.IsTrue(content.First().IsFirst());
            Assert.IsFalse(content.First().IsLast());
            Assert.IsFalse(content.First().Next().IsFirst());
            Assert.IsFalse(content.First().Next().IsLast());
            Assert.IsFalse(content.First().Next().Next().IsFirst());
            Assert.IsTrue(content.First().Next().Next().IsLast());
        }

        [Test]
        [Ignore("Fails as long as PublishedContentModel is internal.")] // fixme
        public void Issue()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot()
                .Distinct()
                .OfType<ContentType2>();

            var where = content.Where(x => x.Prop1 == 1234);
            var first = where.First();
            Assert.AreEqual(1234, first.Prop1);

            var content2 = UmbracoContext.Current.ContentCache.GetAtRoot()
                .OfType<ContentType2>()
                .First(x => x.Prop1 == 1234);
            Assert.AreEqual(1234, content2.Prop1);

            var content3 = UmbracoContext.Current.ContentCache.GetAtRoot()
                .OfType<ContentType2>()
                .First();
            Assert.AreEqual(1234, content3.Prop1);
        }

        static SolidPublishedCaches CreatePublishedContent()
        {
            var caches = new SolidPublishedCaches();
            var cache = caches.ContentCache;

            var props = new[]
                    {
                        new PublishedPropertyType("prop1", 1, "?"), 
                    };

            var contentType1 = new PublishedContentType(1, "ContentType1", props);
            var contentType2 = new PublishedContentType(2, "ContentType2", props);
            var contentType2s = new PublishedContentType(3, "ContentType2Sub", props);

            cache.Add(new SolidPublishedContent(contentType1)
                {
                    Id = 1,
                    SortOrder = 0,
                    Name = "Content 1",
                    UrlName = "content-1",
                    Path = "/1",
                    Level = 1,
                    Url = "/content-1",
                    ParentId = -1,
                    ChildIds = new int[] {},
                    Properties = new Collection<IPublishedProperty>
                        {
                            new SolidPublishedProperty
                                {
                                    PropertyTypeAlias = "prop1",
                                    HasValue = true,
                                    Value = 1234,
                                    DataValue = "1234"
                                }
                        }
                });

            cache.Add(new SolidPublishedContent(contentType2)
                {
                    Id = 2,
                    SortOrder = 1,
                    Name = "Content 2",
                    UrlName = "content-2",
                    Path = "/2",
                    Level = 1,
                    Url = "/content-2",
                    ParentId = -1,
                    ChildIds = new int[] { },
                    Properties = new Collection<IPublishedProperty>
                            {
                                new SolidPublishedProperty
                                    {
                                        PropertyTypeAlias = "prop1",
                                        HasValue = true,
                                        Value = 1234,
                                        DataValue = "1234"
                                    }
                            }
                });

            cache.Add(new SolidPublishedContent(contentType2s)
            {
                Id = 3,
                SortOrder = 2,
                Name = "Content 2Sub",
                UrlName = "content-2sub",
                Path = "/3",
                Level = 1,
                Url = "/content-2sub",
                ParentId = -1,
                ChildIds = new int[] { },
                Properties = new Collection<IPublishedProperty>
                            {
                                new SolidPublishedProperty
                                    {
                                        PropertyTypeAlias = "prop1",
                                        HasValue = true,
                                        Value = 1234,
                                        DataValue = "1234"
                                    }
                            }
            });

            return caches;
        }
    }
}

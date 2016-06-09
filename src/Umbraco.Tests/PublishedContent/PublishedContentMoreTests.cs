using System.Linq;
using System.Collections.ObjectModel;
using System.Web.Routing;
using Moq;
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
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Plugins;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Routing;
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
                new PropertyValueConvertersResolver(Container, Logger);
            var types = PluginManager.Current.ResolveTypes<PublishedContentModel>();
            PublishedContentModelFactoryResolver.Current =
                new PublishedContentModelFactoryResolver(new PublishedContentModelFactory(types));

            base.FreezeResolution();
        }

        private void InitializeUmbracoContext()
        {
            RouteData routeData = null;

            var facade = CreateFacade();

            var facadeService = new Mock<IFacadeService>();
            facadeService.Setup(x => x.CreateFacade(It.IsAny<string>())).Returns(facade);

            var httpContext = GetHttpContextFactory("http://umbraco.local/", routeData).HttpContext;
            var ctx = UmbracoContext.CreateContext(
                httpContext, ApplicationContext,
                facadeService.Object,
                new WebSecurity(httpContext, ApplicationContext),
                Mock.Of<IUmbracoSettingsSection>(),
                Enumerable.Empty<IUrlProvider>());

            Umbraco.Web.Current.SetUmbracoContext(ctx, true);
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
        public void Distinct()
        {
            var items = UmbracoContext.Current.ContentCache.GetAtRoot()
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
            var items = UmbracoContext.Current.ContentCache.GetAtRoot()
                .OfType<ContentType2>()
                .Distinct()
                .ToIndexedArray();
            Assert.AreEqual(2, items.Length);
            Assert.IsInstanceOf<ContentType2>(items.First().Content);
        }

        [Test]
        public void OfType2()
        {
            var content = UmbracoContext.Current.ContentCache.GetAtRoot()
                .OfType<ContentType2Sub>()
                .Distinct()
                .ToIndexedArray();
            Assert.AreEqual(1, content.Length);
            Assert.IsInstanceOf<ContentType2Sub>(content.First().Content);
        }

        [Test]
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
            var items = UmbracoContext.Current.ContentCache.GetAtRoot()
                .Where(x => x.GetPropertyValue<int>("prop1") == 1234)
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

        [Test]
        public void PublishedContentQueryTypedContentList()
        {
            var query = new PublishedContentQuery(UmbracoContext.Current.ContentCache, UmbracoContext.Current.MediaCache);
            var result = query.TypedContent(new[] { 1, 2, 4 }).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[1].Id);
        }

        static SolidFacade CreateFacade()
        {
            var caches = new SolidFacade();
            var cache = caches.InnerContentCache;

            var props = new[]
                    {
                        new PublishedPropertyType("prop1", 1, "?"), 
                    };

            var contentType1 = new PublishedContentType(1, "ContentType1", Enumerable.Empty<string>(), props);
            var contentType2 = new PublishedContentType(2, "ContentType2", Enumerable.Empty<string>(), props);
            var contentType2s = new PublishedContentType(3, "ContentType2Sub", Enumerable.Empty<string>(), props);

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

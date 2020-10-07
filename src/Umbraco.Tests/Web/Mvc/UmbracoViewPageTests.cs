using System;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.Common;
using Umbraco.Tests.LegacyXmlPublishedCache;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class UmbracoViewPageTests : UmbracoTestBase
    {
        private XmlPublishedSnapshotService _service;

        [TearDown]
        public override void TearDown()
        {
            if (_service == null) return;
            _service.Dispose();
            _service = null;
        }

        #region RenderModel To ...

        [Test]
        public void RenderModel_To_RenderModel()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new RenderModelTestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.AreSame(model, view.Model);
        }

        [Test]
        public void RenderModel_ContentType1_To_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new ContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void RenderModel_ContentType2_To_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new ContentModel(content);
            var view = new ContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void RenderModel_ContentType1_To_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new ContentType2TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();

            Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        }

        [Test]
        public void RenderModel_ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void RenderModel_ContentType2_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new ContentModel(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType2>(view.Model.Content);
        }

        [Test]
        public void RenderModel_ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();

            Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        }

        #endregion

        #region RenderModelOf To ...

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModel()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelTestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.AreSame(model, view.Model);
        }

        [Test]
        public void RenderModelOf_ContentType1_To_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new ContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void RenderModelOf_ContentType2_To_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new ContentModel<ContentType2>(content);
            var view = new ContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void RenderModelOf_ContentType1_To_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new ContentType2TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        }

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void RenderModelOf_ContentType2_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new ContentModel<ContentType2>(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType2>(view.Model.Content);
        }

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        }

        #endregion

        #region ContentType To ...

        [Test]
        public void ContentType1_To_RenderModel()
        {
            var content = new ContentType1(null);
            var view = new RenderModelTestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentModel>(view.Model);
        }

        [Test]
        public void ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void ContentType2_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType2(null);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            Assert.Throws<ModelBindingException>(() =>view.SetViewDataX(viewData));
        }

        [Test]
        public void ContentType1_To_ContentType1()
        {
            var content = new ContentType1(null);
            var view = new ContentType1TestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void ContentType1_To_ContentType2()
        {
            var content = new ContentType1(null);
            var view = new ContentType2TestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        }

        [Test]
        public void ContentType2_To_ContentType1()
        {
            var content = new ContentType2(null);
            var view = new ContentType1TestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        #endregion

        #region Test elements

        public class TestPage<TModel> : UmbracoViewPage<TModel>
        {
            public override void Execute()
            {
                throw new NotImplementedException();
            }

            public void SetViewDataX(ViewDataDictionary viewData)
            {
                SetViewData(viewData);
            }
        }

        public class RenderModelTestPage : TestPage<ContentModel>
        { }

        public class RenderModelOfContentType1TestPage : TestPage<ContentModel<ContentType1>>
        { }

        public class RenderModelOfContentType2TestPage : TestPage<ContentModel<ContentType2>>
        { }

        public class ContentType1TestPage : TestPage<ContentType1>
        { }

        public class ContentType2TestPage : TestPage<ContentType2>
        { }

        public class ContentType1 : PublishedContentWrapped
        {
            public ContentType1(IPublishedContent content) : base(content) {}
        }

        public class ContentType2 : ContentType1
        {
            public ContentType2(IPublishedContent content) : base(content) { }
        }

        #endregion

        #region Test helpers

        ServiceContext GetServiceContext()
        {
            return TestObjects.GetServiceContextMock();
        }

        ViewContext GetViewContext()
        {
            var umbracoContext = GetUmbracoContext("/dang", 0);

            var webRoutingSettings = new WebRoutingSettings();
            var publishedRouter = BaseWebTest.CreatePublishedRouter(webRoutingSettings);
            var frequest = publishedRouter.CreateRequest(umbracoContext,  new Uri("http://localhost/dang"));

            frequest.Culture = CultureInfo.InvariantCulture;
            umbracoContext.PublishedRequest = frequest;

            var context = new ViewContext();
            context.RouteData = new RouteData();
            context.RouteData.DataTokens.Add(Core.Constants.Web.UmbracoContextDataToken, umbracoContext);

            return context;
        }

        protected IUmbracoContext GetUmbracoContext(string url, int templateId, RouteData routeData = null, bool setSingleton = false)
        {
            var svcCtx = GetServiceContext();

            var databaseFactory = TestObjects.GetDatabaseFactoryMock();

            //var appCtx = new ApplicationContext(
            //    new DatabaseContext(databaseFactory, logger, Mock.Of<IRuntimeState>(), Mock.Of<IMigrationEntryService>()),
            //    svcCtx,
            //    CacheHelper.CreateDisabledCacheHelper(),
            //    new ProfilingLogger(logger, Mock.Of<IProfiler>())) { /*IsReady = true*/ };

            var cache = NoAppCache.Instance;
            //var provider = new ScopeUnitOfWorkProvider(databaseFactory, new RepositoryFactory(Mock.Of<IServiceContainer>()));
            var scopeProvider = TestObjects.GetScopeProvider(NullLoggerFactory.Instance);
            var factory = Mock.Of<IPublishedContentTypeFactory>();
            var umbracoContextAccessor = Mock.Of<IUmbracoContextAccessor>();
            _service = new XmlPublishedSnapshotService(svcCtx, factory, scopeProvider, cache,
                null, null,
                umbracoContextAccessor, null, null, null,
                new TestDefaultCultureAccessor(),
                Current.LoggerFactory, TestObjects.GetGlobalSettings(),
                TestHelper.GetHostingEnvironment(),
                TestHelper.GetHostingEnvironmentLifetime(),
                ShortStringHelper,
                new SiteDomainHelper(),
                Factory.GetInstance<IEntityXmlSerializer>(),
                null, true, false
                ); // no events

            var http = GetHttpContextFactory(url, routeData).HttpContext;

            var httpContextAccessor = TestHelper.GetHttpContextAccessor(http);
            var globalSettings = TestObjects.GetGlobalSettings();

            var ctx = new UmbracoContext(
                httpContextAccessor,
                _service,
                Mock.Of<IBackofficeSecurity>(),
                globalSettings,
                HostingEnvironment,
                new TestVariationContextAccessor(),
                UriUtility,
                new AspNetCookieManager(httpContextAccessor));

            //if (setSingleton)
            //{
            //    UmbracoContext.Current = ctx;
            //}

            return ctx;
        }

        protected FakeHttpContextFactory GetHttpContextFactory(string url, RouteData routeData = null)
        {
            var factory = routeData != null
                            ? new FakeHttpContextFactory(url, routeData)
                            : new FakeHttpContextFactory(url);

            return factory;
        }

        #endregion
    }
}

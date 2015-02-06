﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using NUnit.Framework;
using Umbraco.Web.Security;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Mvc
{
    [TestFixture]
    public class UmbracoViewPageTests
    {
        #region RenderModel To ...

        [Test]
        public void RenderModel_To_RenderModel()
        {
            var content = new ContentType1(null);
            var model = new RenderModel(content, CultureInfo.InvariantCulture);
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
            var model = new RenderModel(content, CultureInfo.InvariantCulture);
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
            var model = new RenderModel(content, CultureInfo.InvariantCulture);
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
            var model = new RenderModel(content, CultureInfo.InvariantCulture);
            var view = new ContentType2TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();

            Assert.Throws<InvalidCastException>(() => view.SetViewDataX(viewData));
        }

        [Test]
        public void RenderModel_ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new RenderModel(content, CultureInfo.InvariantCulture);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<RenderModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void RenderModel_ContentType2_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new RenderModel(content, CultureInfo.InvariantCulture);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<RenderModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType2>(view.Model.Content);
        }

        [Test]
        public void RenderModel_ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new RenderModel(content, CultureInfo.InvariantCulture);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();

            Assert.Throws<InvalidCastException>(() => view.SetViewDataX(viewData));
        }

        #endregion

        #region RenderModelOf To ...

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModel()
        {
            var content = new ContentType1(null);
            var model = new RenderModel<ContentType1>(content, CultureInfo.InvariantCulture);
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
            var model = new RenderModel<ContentType1>(content, CultureInfo.InvariantCulture);
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
            var model = new RenderModel<ContentType2>(content, CultureInfo.InvariantCulture);
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
            var model = new RenderModel<ContentType1>(content, CultureInfo.InvariantCulture);
            var view = new ContentType2TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            Assert.Throws<InvalidCastException>(() => view.SetViewDataX(viewData));
        }

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new RenderModel<ContentType1>(content, CultureInfo.InvariantCulture);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<RenderModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void RenderModelOf_ContentType2_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new RenderModel<ContentType2>(content, CultureInfo.InvariantCulture);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<RenderModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType2>(view.Model.Content);
        }

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new RenderModel<ContentType1>(content, CultureInfo.InvariantCulture);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = new ViewDataDictionary(model);

            view.ViewContext = GetViewContext();
            Assert.Throws<InvalidCastException>(() => view.SetViewDataX(viewData));
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

            Assert.IsInstanceOf<RenderModel>(view.Model);
        }

        [Test]
        public void ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            view.SetViewDataX(viewData);

            Assert.IsInstanceOf<RenderModel<ContentType1>>(view.Model);
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

            Assert.IsInstanceOf<RenderModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = new ViewDataDictionary(content);

            view.ViewContext = GetViewContext();
            Assert.Throws<InvalidCastException>(() =>view.SetViewDataX(viewData));
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
            Assert.Throws<InvalidCastException>(() => view.SetViewDataX(viewData));
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

        public class RenderModelTestPage : TestPage<RenderModel>
        { }

        public class RenderModelOfContentType1TestPage : TestPage<RenderModel<ContentType1>>
        { }

        public class RenderModelOfContentType2TestPage : TestPage<RenderModel<ContentType2>>
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

        ViewContext GetViewContext()
        {
            var umbracoContext = GetUmbracoContext("/dang", 0);

            var urlProvider = new UrlProvider(umbracoContext, new IUrlProvider[] { new DefaultUrlProvider() });
            var routingContext = new RoutingContext(
                umbracoContext,
                Enumerable.Empty<IContentFinder>(),
                new FakeLastChanceFinder(),
                urlProvider);
            umbracoContext.RoutingContext = routingContext;

            var request = new PublishedContentRequest(new Uri("http://localhost/dang"), routingContext);
            request.Culture = CultureInfo.InvariantCulture;
            umbracoContext.PublishedContentRequest = request;

            var context = new ViewContext();
            context.RouteData = new RouteData();
            context.RouteData.DataTokens.Add("umbraco-context", umbracoContext);

            return context;
        }

        protected UmbracoContext GetUmbracoContext(string url, int templateId, RouteData routeData = null, bool setSingleton = false)
        {
            var cache = new PublishedContentCache();

            //cache.GetXmlDelegate = (context, preview) =>
            //{
            //    var doc = new XmlDocument();
            //    doc.LoadXml(GetXmlContent(templateId));
            //    return doc;
            //};

            //PublishedContentCache.UnitTesting = true;

            // ApplicationContext.Current = new ApplicationContext(false) { IsReady = true };
            var appCtx = new ApplicationContext(CacheHelper.CreateDisabledCacheHelper()) { IsReady = true };
            var http = GetHttpContextFactory(url, routeData).HttpContext;
            var ctx = new UmbracoContext(
                GetHttpContextFactory(url, routeData).HttpContext,
                appCtx,
                new PublishedCaches(cache, new PublishedMediaCache(appCtx)),
                new WebSecurity(http, appCtx));

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


            //set the state helper
            StateHelper.HttpContext = factory.HttpContext;

            return factory;
        }

        #endregion
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Models;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common.Views
{
    [TestFixture]
    public class UmbracoViewPageTests
    {
        #region RenderModel To ...
        [Test]
        public void RenderModel_To_RenderModel()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new RenderModelTestPage();
            var viewData = GetViewDataDictionary<ContentModel>(model);
            view.ViewData = viewData;

            Assert.AreSame(model, view.Model);
        }

        [Test]
        public void RenderModel_ContentType1_To_ContentType1()
        {
            var content = new ContentType1(null);
            var view = new ContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentType1>(content);

            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void RenderModel_ContentType2_To_ContentType1()
        {
            var content = new ContentType2(null);
            var view = new ContentType1TestPage();

            var viewData = GetViewDataDictionary<ContentType1>(content);
            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        // [Test]
        // public void RenderModel_ContentType1_To_ContentType2()
        // {
        //     // everything is strongly typed, so I'm not even allowed to try and set the ViewData with the wrong type
        //     var content = new ContentType1(null);
        //     var model = new ContentModel(content);
        //     var view = new ContentType2TestPage();
        //     var viewData = new ViewDataDictionary(model);
        //
        //     view.ViewContext = GetViewContext();
        //
        //     Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        // }

        [Test]
        public void RenderModel_ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);

            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void RenderModel_ContentType2_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);
            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType2>(view.Model.Content);
        }

        // [Test]
        // public void RenderModel_ContentType1_To_RenderModelOf_ContentType2()
        // {
        //     // everything is strongly typed, so I'm not even allowed to try and create the ContentModel with the wrong type
        //     var content = new ContentType1(null);
        //     var model = new ContentModel(content);
        //     var view = new RenderModelOfContentType2TestPage();
        //     var viewData = new ViewDataDictionary(model);
        //
        //     view.ViewContext = GetViewContext();
        //
        //     Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        // }

        #endregion

        #region RenderModelOf To ...

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModel()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelTestPage();
            var viewData = GetViewDataDictionary<ContentModel>(model);

            view.ViewData = viewData;

            Assert.AreSame(model, view.Model);
        }

        // [Test]
        // public void RenderModelOf_ContentType1_To_ContentType1()
        // {
        //     // Can't create viewdata with ContentType1 from  ContentModel<ContentType1> because it doesn't actually inherit ContentType1
        //     // And can't set viewdata from ContentModel<ContentType1> because the page expects ContentType1 and not ContentModel
        //     // And if I change it the test will be the same as RenderModel_ContentType1_To_ContentType1
        //     var content = new ContentType1(null);
        //     var model = new ContentModel<ContentType1>(content);
        //     var view = new ContentType1TestPage();
        //     var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);
        //
        //     view.ViewData = viewData;
        //
        //     Assert.IsInstanceOf<ContentType1>(view.Model);
        // }

        // [Test]
        // public void RenderModelOf_ContentType2_To_ContentType1()
        // {
        //     // Same issue as above test
        //     var content = new ContentType2(null);
        //     var model = new ContentModel<ContentType2>(content);
        //     var view = new ContentType1TestPage();
        //     var viewData = new ViewDataDictionary(model);
        //
        //     view.ViewContext = GetViewContext();
        //     view.SetViewDataX(viewData);
        //
        //     Assert.IsInstanceOf<ContentType1>(view.Model);
        // }

        // [Test]
        // public void RenderModelOf_ContentType1_To_ContentType2()
        // {
        //     // Same issue as above, and as RenderModel1_ContentType1_To_ContentType2
        //     var content = new ContentType1(null);
        //     var model = new ContentModel<ContentType1>(content);
        //     var view = new ContentType2TestPage();
        //     var viewData = new ViewDataDictionary(model);
        //
        //     view.ViewContext = GetViewContext();
        //     Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        // }

        // [Test]
        // public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType1()
        // {
        //     // It's the same as RenderModel_ContentType1_To_RenderModelOf_ContentType1
        //     var content = new ContentType1(null);
        //     var model = new ContentModel<ContentType1>(content);
        //     var view = new RenderModelOfContentType1TestPage();
        //     var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);
        //
        //     view.ViewData = viewData;
        //
        //     Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        //     Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        // }

        // [Test]
        // public void RenderModelOf_ContentType2_To_RenderModelOf_ContentType1()
        // {
        //     // Same as RenderModel_ContentType2_To_RenderModelOf_ContentType1 after merge
        //     var content = new ContentType2(null);
        //     var model = new ContentModel<ContentType1>(content);
        //     var view = new RenderModelOfContentType1TestPage();
        //     var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);
        //
        //     view.ViewData = viewData;
        //
        //     Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        //     Assert.IsInstanceOf<ContentType2>(view.Model.Content);
        // }

        // [Test]
        // public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType2()
        // {
        //     // Same issue as RenderModel1_ContentType1_To_ContentType2
        //     var content = new ContentType1(null);
        //     var model = new ContentModel<ContentType1>(content);
        //     var view = new RenderModelOfContentType2TestPage();
        //     var viewData = new ViewDataDictionary(model);
        //
        //     view.ViewContext = GetViewContext();
        //     Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        // }

        #endregion

        #region ContentType To ...

        // [Test]
        // public void ContentType1_To_RenderModel()
        // {
        //     // ContentType1 cannot be sat as ViewData because ContentModel is expected
        //     var content = new ContentType1(null);
        //     var view = new RenderModelTestPage();
        //
        //     var viewData = GetViewDataDictionary<ContentType1>(content);
        //
        //     view.ViewData = viewData;
        //
        //     Assert.IsInstanceOf<ContentModel>(view.Model);
        // }

        // [Test]
        // public void ContentType1_To_RenderModelOf_ContentType1()
        // {
        //     // same as above but with ContentModel<ContentType1> instead of ContentModel
        //     var content = new ContentType1(null);
        //     var view = new RenderModelOfContentType1TestPage();
        //
        //     var viewData = GetViewDataDictionary<ContentType1>(content);
        //     view.ViewData = viewData;
        //
        //     Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        //     Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        // }

        // [Test]
        // public void ContentType2_To_RenderModelOf_ContentType1()
        // {
        //     // Same as above but with ContentModel<ContentType2>
        //     var content = new ContentType2(null);
        //     var view = new RenderModelOfContentType1TestPage();
        //     var viewData = GetViewDataDictionary<ContentType2>(content);
        //
        //     view.ViewData = viewData;
        //
        //     Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        //     Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        // }

        // [Test]
        // public void ContentType1_To_RenderModelOf_ContentType2()
        // {
        //     // Same as above, and as RenderModel1_ContentType1_To_ContentType2
        //     var content = new ContentType1(null);
        //     var view = new RenderModelOfContentType2TestPage();
        //     var viewData = new ViewDataDictionary(content);
        //
        //     view.ViewContext = GetViewContext();
        //     Assert.Throws<ModelBindingException>(() =>view.SetViewDataX(viewData));
        // }

        // [Test]
        // public void ContentType1_To_ContentType1()
        // {
        //     // Same as ContentType1_To_ContentType1
        //     var content = new ContentType1(null);
        //     var view = new ContentType1TestPage();
        //     var viewdata = GetViewDataDictionary<ContentType1>(content);
        //
        //     view.ViewData = viewdata;
        //
        //     Assert.IsInstanceOf<ContentType1>(view.Model);
        // }

        // [Test]
        // public void ContentType1_To_ContentType2()
        // {
        //     // Same issue as RenderModel1_ContentType1_To_ContentType2
        //     var content = new ContentType1(null);
        //     var view = new ContentType2TestPage();
        //     var viewData = new ViewDataDictionary(content);
        //
        //     view.ViewContext = GetViewContext();
        //     Assert.Throws<ModelBindingException>(() => view.SetViewDataX(viewData));
        // }

        // [Test]
        // public void ContentType2_To_ContentType1()
        // {
        //     // Will be the same as RenderModel_ContentType2_To_ContentType1 after merge
        //     var content = new ContentType2(null);
        //     var view = new ContentType1TestPage();
        //     var viewData = new ViewDataDictionary(content);
        //
        //     view.ViewContext = GetViewContext();
        //     view.SetViewDataX(viewData);
        //
        //     Assert.IsInstanceOf<ContentType1>(view.Model);
        // }

        #endregion

        #region Test helpers methods

        private ViewDataDictionary<T> GetViewDataDictionary<T>(object model)
        {
            var sourceViewDataDictionary = new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            return new ViewDataDictionary<T>(sourceViewDataDictionary, model);
        }


        #endregion

        #region Test elements

        public class ContentType1 : PublishedContentWrapped
        {
            public ContentType1(IPublishedContent content) : base(content) {}
        }

        public class ContentType2 : ContentType1
        {
            public ContentType2(IPublishedContent content) : base(content) { }
        }

        public class TestPage<TModel> : UmbracoViewPage<TModel>
        {
            public override Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }
        }

        public class RenderModelTestPage : TestPage<ContentModel>
        { }

        public class ContentType1TestPage : TestPage<ContentType1>
        { }

        public class ContentType2TestPage : TestPage<ContentType2>
        { }

        public class RenderModelOfContentType1TestPage : TestPage<ContentModel<ContentType1>>
        { }

        public class RenderModelOfContentType2TestPage : TestPage<ContentModel<ContentType2>>
        { }

        #endregion
    }
}

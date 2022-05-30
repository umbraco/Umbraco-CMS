// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Cms.Web.Common.Views;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Views;

[TestFixture]
public class UmbracoViewPageTests
{
    [Test]
    public void RenderModel_To_RenderModel()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel(content);
        var view = new RenderModelTestPage();
        var viewData = GetViewDataDictionary<ContentModel>(model);
        view.ViewData = viewData;

        Assert.AreSame(model, view.Model);
    }

    [Test]
    public void RenderModel_ContentType1_To_ContentType1()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var view = new ContentType1TestPage();
        var viewData = GetViewDataDictionary<ContentType1>(content);

        view.ViewData = viewData;

        Assert.IsInstanceOf<ContentType1>(view.Model);
    }

    [Test]
    public void RenderModel_ContentType2_To_ContentType1()
    {
        var content = new ContentType2(null, Mock.Of<IPublishedValueFallback>());
        var view = new ContentType1TestPage();

        var viewData = GetViewDataDictionary<ContentType1>(content);
        view.ViewData = viewData;

        Assert.IsInstanceOf<ContentType1>(view.Model);
    }

    [Test]
    public void RenderModel_ContentType1_To_ContentType2()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel(content);
        var view = new ContentType2TestPage();
        var viewData = GetViewDataDictionary(model);

        Assert.Throws<ModelBindingException>(() => view.SetViewData(viewData));
    }

    [Test]
    public void RenderModel_ContentType1_To_RenderModelOf_ContentType1()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
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
        var content = new ContentType2(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel<ContentType1>(content);
        var view = new RenderModelOfContentType1TestPage();
        var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);
        view.ViewData = viewData;

        Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        Assert.IsInstanceOf<ContentType2>(view.Model.Content);
    }

    [Test]
    public void RenderModel_ContentType1_To_RenderModelOf_ContentType2()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel(content);
        var view = new RenderModelOfContentType2TestPage();
        var viewData = GetViewDataDictionary(model);

        Assert.Throws<ModelBindingException>(() => view.SetViewData(viewData));
    }

    [Test]
    public void RenderModelOf_ContentType1_To_RenderModel()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel<ContentType1>(content);
        var view = new RenderModelTestPage();
        var viewData = GetViewDataDictionary<ContentModel>(model);

        view.ViewData = viewData;

        Assert.AreSame(model, view.Model);
    }

    [Test]
    public void RenderModelOf_ContentType1_To_ContentType1()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel<ContentType1>(content);
        var view = new ContentType1TestPage();
        var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);

        view.SetViewData(viewData);

        Assert.IsInstanceOf<ContentType1>(view.Model);
    }

    [Test]
    public void RenderModelOf_ContentType2_To_ContentType1()
    {
        var content = new ContentType2(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel<ContentType2>(content);
        var view = new ContentType1TestPage();
        var viewData =
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };

        view.SetViewData(viewData);

        Assert.IsInstanceOf<ContentType1>(view.Model);
    }

    [Test]
    public void RenderModelOf_ContentType1_To_ContentType2()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel<ContentType1>(content);
        var view = new ContentType2TestPage();
        var viewData = GetViewDataDictionary(model);

        Assert.Throws<ModelBindingException>(() => view.SetViewData(viewData));
    }

    [Test]
    public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType1()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel<ContentType1>(content);
        var view = new RenderModelOfContentType1TestPage();
        var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);

        view.ViewData = viewData;

        Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        Assert.IsInstanceOf<ContentType1>(view.Model.Content);
    }

    [Test]
    public void RenderModelOf_ContentType2_To_RenderModelOf_ContentType1()
    {
        var content = new ContentType2(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel<ContentType1>(content);
        var view = new RenderModelOfContentType1TestPage();
        var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);

        view.SetViewData(viewData);

        Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        Assert.IsInstanceOf<ContentType2>(view.Model.Content);
    }

    [Test]
    public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType2()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var model = new ContentModel<ContentType1>(content);
        var view = new RenderModelOfContentType2TestPage();
        var viewData = GetViewDataDictionary(model);

        Assert.Throws<ModelBindingException>(() => view.SetViewData(viewData));
    }

    [Test]
    public void ContentType1_To_RenderModel()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var view = new RenderModelTestPage();

        var viewData = GetViewDataDictionary<ContentType1>(content);

        view.SetViewData(viewData);

        Assert.IsInstanceOf<ContentModel>(view.Model);
    }

    [Test]
    public void ContentType1_To_RenderModelOf_ContentType1()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var view = new RenderModelOfContentType1TestPage();

        var viewData = GetViewDataDictionary<ContentType1>(content);
        view.SetViewData(viewData);

        Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        Assert.IsInstanceOf<ContentType1>(view.Model.Content);
    }

    [Test]
    public void ContentType2_To_RenderModelOf_ContentType1()
    {
        // Same as above but with ContentModel<ContentType2>
        var content = new ContentType2(null, Mock.Of<IPublishedValueFallback>());
        var view = new RenderModelOfContentType1TestPage();
        var viewData = GetViewDataDictionary<ContentType2>(content);

        view.SetViewData(viewData);

        Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
        Assert.IsInstanceOf<ContentType1>(view.Model.Content);
    }

    [Test]
    public void ContentType1_To_RenderModelOf_ContentType2()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var view = new RenderModelOfContentType2TestPage();
        var viewData = GetViewDataDictionary(content);

        Assert.Throws<ModelBindingException>(() => view.SetViewData(viewData));
    }

    [Test]
    public void ContentType1_To_ContentType1()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var view = new ContentType1TestPage();
        var viewData = GetViewDataDictionary<ContentType1>(content);

        view.SetViewData(viewData);

        Assert.IsInstanceOf<ContentType1>(view.Model);
    }

    [Test]
    public void ContentType1_To_ContentType2()
    {
        var content = new ContentType1(null, Mock.Of<IPublishedValueFallback>());
        var view = new ContentType2TestPage();
        var viewData = GetViewDataDictionary(content);

        Assert.Throws<ModelBindingException>(() => view.SetViewData(viewData));
    }

    [Test]
    public void ContentType2_To_ContentType1()
    {
        var content = new ContentType2(null, Mock.Of<IPublishedValueFallback>());
        var view = new ContentType1TestPage();
        var viewData = GetViewDataDictionary(content);

        view.SetViewData(viewData);

        Assert.IsInstanceOf<ContentType1>(view.Model);
    }

    private ViewDataDictionary<T> GetViewDataDictionary<T>(object model)
    {
        var sourceViewDataDictionary =
            new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        return new ViewDataDictionary<T>(sourceViewDataDictionary, model);
    }

    private ViewDataDictionary GetViewDataDictionary(object model)
    {
        var sourceViewDataDictionary =
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        return new ViewDataDictionary(sourceViewDataDictionary) { Model = model };
    }

    public class ContentType1 : PublishedContentWrapped
    {
        public ContentType1(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }
    }

    public class ContentType2 : ContentType1
    {
        public ContentType2(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }
    }

    public class TestPage<TModel> : UmbracoViewPage<TModel>
    {
        private readonly ContentModelBinder _modelBinder = new(Mock.Of<IEventAggregator>());

        public override Task ExecuteAsync() => throw new NotImplementedException();

        public void SetViewData(ViewDataDictionary viewData) =>
            ViewData = (ViewDataDictionary<TModel>)BindViewData(_modelBinder, viewData);
    }

    public class RenderModelTestPage : TestPage<ContentModel>
    {
    }

    public class ContentType1TestPage : TestPage<ContentType1>
    {
    }

    public class ContentType2TestPage : TestPage<ContentType2>
    {
    }

    public class RenderModelOfContentType1TestPage : TestPage<ContentModel<ContentType1>>
    {
    }

    public class RenderModelOfContentType2TestPage : TestPage<ContentModel<ContentType2>>
    {
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Common.ModelBinders;
using Umbraco.Web.Models;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common.ModelBinders
{
    [TestFixture]
    public class RenderModelBinderTests
    {
        private ContentModelBinder _contentModelBinder;

        [SetUp]
        public void SetUp() => _contentModelBinder = new ContentModelBinder();

        [Test]
        [TestCase(typeof(IPublishedContent), false)]
        [TestCase(typeof(ContentModel), false)]
        [TestCase(typeof(MyContent), false)]
        [TestCase(typeof(ContentModel<MyContent>), false)]
        [TestCase(typeof(MyOtherContent), true)]
        [TestCase(typeof(MyCustomContentModel), true)]
        [TestCase(typeof(IContentModel), true)]
        public void Returns_Binder_For_IPublishedContent_And_IRenderModel(Type testType, bool expectNull)
        {
            var binderProvider = new ContentModelBinderProvider();
            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(x => x.Metadata).Returns(new EmptyModelMetadataProvider().GetMetadataForType(testType));

            IModelBinder found = binderProvider.GetBinder(contextMock.Object);
            if (expectNull)
            {
                Assert.IsNull(found);
            }
            else
            {
                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void BindModel_Null_Source_Returns_Null()
        {
            var bindingContext = new DefaultModelBindingContext();
            _contentModelBinder.BindModelAsync(bindingContext, null, typeof(MyContent));
            Assert.IsNull(bindingContext.Result.Model);
        }

        [Test]
        public void BindModel_Returns_If_Same_Type()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bindingContext = new DefaultModelBindingContext();

            _contentModelBinder.BindModelAsync(bindingContext, content, typeof(MyContent));

            Assert.AreSame(content, bindingContext.Result.Model);
        }

        [Test]
        public void BindModel_RenderModel_To_IPublishedContent()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var renderModel = new ContentModel(content);

            var bindingContext = new DefaultModelBindingContext();
            _contentModelBinder.BindModelAsync(bindingContext, renderModel, typeof(IPublishedContent));

            Assert.AreSame(content, bindingContext.Result.Model);
        }

        [Test]
        public void BindModel_IPublishedContent_To_RenderModel()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bindingContext = new DefaultModelBindingContext();

            _contentModelBinder.BindModelAsync(bindingContext, content, typeof(ContentModel));

            var bound = (IContentModel)bindingContext.Result.Model;
            Assert.AreSame(content, bound.Content);
        }

        [Test]
        public void BindModel_IPublishedContent_To_Generic_RenderModel()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bindingContext = new DefaultModelBindingContext();

            _contentModelBinder.BindModelAsync(bindingContext, content, typeof(ContentModel<MyContent>));

            var bound = (IContentModel)bindingContext.Result.Model;
            Assert.AreSame(content, bound.Content);
        }

        [Test]
        public void No_DataToken_Returns_Null()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            ModelBindingContext bindingContext = CreateBindingContext(typeof(ContentModel), false, content);

            _contentModelBinder.BindModelAsync(bindingContext);

            Assert.IsNull(bindingContext.Result.Model);
        }

        [Test]
        public void Invalid_DataToken_Model_Type_Returns_Null()
        {
            ModelBindingContext bindingContext = CreateBindingContext(typeof(IPublishedContent), source: "Hello");
            _contentModelBinder.BindModelAsync(bindingContext);
            Assert.IsNull(bindingContext.Result.Model);
        }

        [Test]
        public void IPublishedContent_DataToken_Model_Type_Uses_DefaultImplementation()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            ModelBindingContext bindingContext = CreateBindingContext(typeof(MyContent), source: content);

            _contentModelBinder.BindModelAsync(bindingContext);

            Assert.AreEqual(content, bindingContext.Result.Model);
        }

        private ModelBindingContext CreateBindingContext(Type modelType, bool withUmbracoDataToken = true, object source = null)
        {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            if (withUmbracoDataToken)
            {
                routeData.DataTokens.Add(Constants.Web.UmbracoDataToken, source);
            }

            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
            var metadataProvider = new EmptyModelMetadataProvider();
            var routeValueDictionary = new RouteValueDictionary();
            var valueProvider = new RouteValueProvider(BindingSource.Path, routeValueDictionary);
            return new DefaultModelBindingContext
            {
                ActionContext = actionContext,
                ModelMetadata = metadataProvider.GetMetadataForType(modelType),
                ModelName = modelType.Name,
                ValueProvider = valueProvider,
            };
        }

        public class MyCustomContentModel : ContentModel
        {
            public MyCustomContentModel(IPublishedContent content)
                : base(content)
            {
            }
        }

        public class MyOtherContent
        {
        }

        public class MyContent : PublishedContentWrapped
        {
            public MyContent(IPublishedContent content)
                : base(content)
            {
            }
        }
    }
}

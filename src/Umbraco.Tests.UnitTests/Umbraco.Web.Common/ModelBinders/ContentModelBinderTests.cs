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
    public class ContentModelBinderTests
    {
        [Test]
        public void Does_Not_Bind_Model_When_UmbracoDataToken_Not_In_Route_Data()
        {
            // Arrange
            var bindingContext = CreateBindingContext(typeof(ContentModel), withUmbracoDataToken: false);
            var binder = new ContentModelBinder();

            // Act
            binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(bindingContext.Result.IsModelSet);
        }

        [Test]
        public void Does_Not_Bind_Model_When_Source_Not_Of_Expected_Type()
        {
            // Arrange
            var bindingContext = CreateBindingContext(typeof(ContentModel), source: new NonContentModel());
            var binder = new ContentModelBinder();

            // Act
            binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(bindingContext.Result.IsModelSet);
        }

        [Test]
        public void BindModel_Returns_If_Same_Type()
        {
            // Arrange
            var content = new ContentModel(CreatePublishedContent());
            var bindingContext = CreateBindingContext(typeof(ContentModel), source: content);
            var binder = new ContentModelBinder();

            // Act
            binder.BindModelAsync(bindingContext);

            // Assert
            Assert.AreSame(content, bindingContext.Result.Model);
        }

        [Test]
        public void Binds_From_IPublishedContent_To_Content_Model()
        {
            // Arrange
            var bindingContext = CreateBindingContext(typeof(ContentModel), source: CreatePublishedContent());
            var binder = new ContentModelBinder();

            // Act
            binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(bindingContext.Result.IsModelSet);
        }

        [Test]
        public void Binds_From_IPublishedContent_To_Content_Model_Of_T()
        {
            // Arrange
            var bindingContext = CreateBindingContext(typeof(ContentModel<ContentType1>), source: new ContentModel<ContentType2>(new ContentType2(CreatePublishedContent())));
            var binder = new ContentModelBinder();

            // Act
            binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(bindingContext.Result.IsModelSet);
        }

        private ModelBindingContext CreateBindingContext(Type modelType, bool withUmbracoDataToken = true, object source = null)
        {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            if (withUmbracoDataToken)
                routeData.DataTokens.Add(Constants.Web.UmbracoDataToken, source);

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

        private class NonContentModel
        {
        }

        private IPublishedContent CreatePublishedContent()
        {
            return new ContentType2(new Mock<IPublishedContent>().Object);
        }

        public class ContentType1 : PublishedContentWrapped
        {
            public ContentType1(IPublishedContent content) : base(content) { }
        }

        public class ContentType2 : ContentType1
        {
            public ContentType2(IPublishedContent content) : base(content) { }
        }
    }
}

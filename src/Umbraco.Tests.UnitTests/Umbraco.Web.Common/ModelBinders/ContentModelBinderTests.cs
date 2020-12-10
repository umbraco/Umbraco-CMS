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
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Models;
using Umbraco.Web.Website.Routing;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common.ModelBinders
{
    [TestFixture]
    public class ContentModelBinderTests
    {
        [Test]
        public void Does_Not_Bind_Model_When_UmbracoDataToken_Not_In_Route_Data()
        {
            // Arrange
            IPublishedContent pc = CreatePublishedContent();
            var bindingContext = CreateBindingContext(typeof(ContentModel), pc, withUmbracoDataToken: false);
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
            IPublishedContent pc = CreatePublishedContent();
            var bindingContext = CreateBindingContext(typeof(ContentModel), pc, source: new NonContentModel());
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
            IPublishedContent pc = CreatePublishedContent();
            var content = new ContentModel(pc);
            var bindingContext = CreateBindingContext(typeof(ContentModel), pc, source: content);
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
            IPublishedContent pc = CreatePublishedContent();
            var bindingContext = CreateBindingContext(typeof(ContentModel), pc, source: pc);
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
            IPublishedContent pc = CreatePublishedContent();
            var bindingContext = CreateBindingContext(typeof(ContentModel<ContentType1>), pc, source: new ContentModel<ContentType2>(new ContentType2(pc)));
            var binder = new ContentModelBinder();

            // Act
            binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(bindingContext.Result.IsModelSet);
        }

        private ModelBindingContext CreateBindingContext(Type modelType, IPublishedContent publishedContent, bool withUmbracoDataToken = true, object source = null)
        {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            if (withUmbracoDataToken)
            {
                routeData.Values.Add(Constants.Web.UmbracoRouteDefinitionDataToken, new UmbracoRouteValues(publishedContent));
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

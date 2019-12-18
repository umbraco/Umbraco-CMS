using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class RenderModelBinderTests
    {
        [SetUp]
        public void SetUp()
        {
            Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        [Test]
        public void Returns_Binder_For_IPublishedContent_And_IRenderModel()
        {
            var binder = ContentModelBinder.Instance;
            var found = binder.GetBinder(typeof (IPublishedContent));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(ContentModel));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(MyContent));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(ContentModel<MyContent>));
            Assert.IsNotNull(found);

            found = binder.GetBinder(typeof(MyOtherContent));
            Assert.IsNull(found);
            found = binder.GetBinder(typeof(MyCustomContentModel));
            Assert.IsNull(found);
            found = binder.GetBinder(typeof(IContentModel));
            Assert.IsNull(found);
        }

        [Test]
        public void BindModel_Null_Source_Returns_Null()
        {
            Assert.IsNull(ContentModelBinder.BindModel(null, typeof(MyContent)));
        }

        [Test]
        public void BindModel_Returns_If_Same_Type()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bound = ContentModelBinder.BindModel(content, typeof (IPublishedContent));
            Assert.AreSame(content, bound);
        }

        [Test]
        public void BindModel_RenderModel_To_IPublishedContent()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var renderModel = new ContentModel(content);
            var bound = ContentModelBinder.BindModel(renderModel, typeof(IPublishedContent));
            Assert.AreSame(content, bound);
        }

        [Test]
        public void BindModel_IPublishedContent_To_RenderModel()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bound = (IContentModel)ContentModelBinder.BindModel(content, typeof(ContentModel));
            Assert.AreSame(content, bound.Content);
        }

        [Test]
        public void BindModel_IPublishedContent_To_Generic_RenderModel()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bound = (IContentModel)ContentModelBinder.BindModel(content, typeof(ContentModel<MyContent>));
            Assert.AreSame(content, bound.Content);
        }

        [Test]
        public void No_DataToken_Returns_Null()
        {
            var binder = ContentModelBinder.Instance;
            var routeData = new RouteData();
            var result = binder.BindModel(new ControllerContext(Mock.Of<HttpContextBase>(), routeData, Mock.Of<ControllerBase>()),
                new ModelBindingContext());

            Assert.IsNull(result);
        }

        [Test]
        public void Invalid_DataToken_Model_Type_Returns_Null()
        {
            var binder = ContentModelBinder.Instance;
            var routeData = new RouteData();
            routeData.DataTokens[Core.Constants.Web.UmbracoDataToken] = "hello";

            //the value provider is the default implementation
            var valueProvider = new Mock<IValueProvider>();
            //also IUnvalidatedValueProvider
            var invalidatedValueProvider = valueProvider.As<IUnvalidatedValueProvider>();
            invalidatedValueProvider.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<bool>())).Returns(() =>
                new ValueProviderResult(null, "", CultureInfo.CurrentCulture));

            var controllerCtx = new ControllerContext(
                Mock.Of<HttpContextBase>(http => http.Items == new Dictionary<object, object>()),
                routeData,
                Mock.Of<ControllerBase>());

            var result = binder.BindModel(controllerCtx,
                new ModelBindingContext
                {
                    ValueProvider = valueProvider.Object,
                    ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, () => null, typeof(IPublishedContent), "content")
                });

            Assert.IsNull(result);
        }

        [Test]
        public void IPublishedContent_DataToken_Model_Type_Uses_DefaultImplementation()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var binder = ContentModelBinder.Instance;
            var routeData = new RouteData();
            routeData.DataTokens[Core.Constants.Web.UmbracoDataToken] = content;

            //the value provider is the default implementation
            var valueProvider = new Mock<IValueProvider>();
            //also IUnvalidatedValueProvider
            var invalidatedValueProvider = valueProvider.As<IUnvalidatedValueProvider>();
            invalidatedValueProvider.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<bool>())).Returns(() =>
                new ValueProviderResult(content, "content", CultureInfo.CurrentCulture));

            var controllerCtx = new ControllerContext(
                Mock.Of<HttpContextBase>(http => http.Items == new Dictionary<object, object>()),
                routeData,
                Mock.Of<ControllerBase>());
            var result = binder.BindModel(controllerCtx,
                new ModelBindingContext
                {
                    ValueProvider = valueProvider.Object,
                    ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, () => null, typeof(IPublishedContent), "content")
                });

            Assert.AreEqual(content, result);
        }

        public class MyCustomContentModel : ContentModel
        {
            public MyCustomContentModel(IPublishedContent content)
                : base(content)
            { }
        }

        public class MyOtherContent
        {

        }

        public class MyContent : PublishedContentWrapped
        {
            public MyContent(IPublishedContent content) : base(content)
            {
            }
        }
    }
}

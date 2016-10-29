using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class RenderModelBinderTests
    {
        [Test]
        public void Returns_Binder_For_IPublishedContent_And_IRenderModel()
        {
            var binder = new RenderModelBinder();
            var found = binder.GetBinder(typeof (IPublishedContent));
            Assert.IsNotNull(found);            
            found = binder.GetBinder(typeof(RenderModel));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(DynamicPublishedContent));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(MyContent));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(RenderModel<MyContent>));
            Assert.IsNotNull(found);

            found = binder.GetBinder(typeof(MyOtherContent));
            Assert.IsNull(found);
            found = binder.GetBinder(typeof(MyCustomRenderModel));
            Assert.IsNull(found);            
            found = binder.GetBinder(typeof(IRenderModel));
            Assert.IsNull(found);
        }

        [Test]
        public void BindModel_Null_Source_Returns_Null()
        {
            Assert.IsNull(RenderModelBinder.BindModel(null, typeof(MyContent), CultureInfo.CurrentCulture));
        }

        [Test]
        public void BindModel_Returns_If_Same_Type()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bound = RenderModelBinder.BindModel(content, typeof (IPublishedContent), CultureInfo.CurrentCulture);
            Assert.AreSame(content, bound);
        }

        [Test]
        public void BindModel_RenderModel_To_IPublishedContent()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var renderModel = new RenderModel(content, CultureInfo.CurrentCulture);
            var bound = RenderModelBinder.BindModel(renderModel, typeof(IPublishedContent), CultureInfo.CurrentCulture);
            Assert.AreSame(content, bound);
        }

        [Test]
        public void BindModel_IPublishedContent_To_RenderModel()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bound = (IRenderModel)RenderModelBinder.BindModel(content, typeof(RenderModel), CultureInfo.CurrentCulture);
            Assert.AreSame(content, bound.Content);
        }

        [Test]
        public void BindModel_IPublishedContent_To_Generic_RenderModel()
        {
            var content = new MyContent(Mock.Of<IPublishedContent>());
            var bound = (IRenderModel)RenderModelBinder.BindModel(content, typeof(RenderModel<MyContent>), CultureInfo.CurrentCulture);
            Assert.AreSame(content, bound.Content);
        }

        [Test]
        public void No_DataToken_Returns_Null()
        {
            var binder = new RenderModelBinder();
            var routeData = new RouteData();
            var result = binder.BindModel(new ControllerContext(Mock.Of<HttpContextBase>(), routeData, Mock.Of<ControllerBase>()),
                new ModelBindingContext());

            Assert.IsNull(result);
        }

        [Test]
        public void Invalid_DataToken_Model_Type_Returns_Null()
        {
            var binder = new RenderModelBinder();
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
            var binder = new RenderModelBinder();
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

        public class MyCustomRenderModel : RenderModel
        {           
            public MyCustomRenderModel(IPublishedContent content, CultureInfo culture) : base(content, culture)
            {
            }            
            public MyCustomRenderModel(IPublishedContent content) : base(content)
            {
            }
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
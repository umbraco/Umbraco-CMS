using System.Globalization;
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
            found = binder.GetBinder(typeof(IRenderModel));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(RenderModel));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(DynamicPublishedContent));
            Assert.IsNotNull(found);
            found = binder.GetBinder(typeof(MyContent));
            Assert.IsNotNull(found);

            found = binder.GetBinder(typeof(MyOtherContent));
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
using System.Linq;
using LightInject;
using NUnit.Framework;
using Umbraco.Core.Macros;
using Umbraco.Web;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class XsltExtensionCollectionTests : ComposingTestBase
    {
        [Test]
        public void XsltExtensionsCollectionBuilderWorks()
        {
            var container = new ServiceContainer();
            var builder = new XsltExtensionCollectionBuilder(container);
            builder.AddExtensionObjectProducer(() => TypeLoader.GetXsltExtensions());
            var extensions = builder.CreateCollection();

            Assert.AreEqual(3, extensions.Count());

            Assert.IsTrue(extensions.Select(x => x.ExtensionObject.GetType()).Contains(typeof (XsltEx1)));
            Assert.IsTrue(extensions.Select(x => x.ExtensionObject.GetType()).Contains(typeof(XsltEx2)));
            Assert.AreEqual("test1", extensions.Single(x => x.ExtensionObject.GetType() == typeof(XsltEx1)).Namespace);
            Assert.AreEqual("test2", extensions.Single(x => x.ExtensionObject.GetType() == typeof(XsltEx2)).Namespace);
        }

        #region Test Objects

        [XsltExtension("test1")]
        public class XsltEx1
        { }

        //test with legacy one
        [umbraco.XsltExtension("test2")]
        public class XsltEx2
        { }

        #endregion
    }
}

using System;
using System.Linq;
using System.Xml;
using LightInject;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class PackageActionCollectionTests : ComposingTestBase
    {
        [Test]
        public void PackageActionCollectionBuilderWorks()
        {
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            container.RegisterCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(() => TypeLoader.GetPackageActions());

            var actions = Current.PackageActions;
            Assert.AreEqual(2, actions.Count());

            // order is unspecified, but both must be there
            bool hasAction1 = actions.ElementAt(0) is PackageAction1 || actions.ElementAt(1) is PackageAction1;
            bool hasAction2 = actions.ElementAt(0) is PackageAction2 || actions.ElementAt(1) is PackageAction2;
            Assert.IsTrue(hasAction1);
            Assert.IsTrue(hasAction2);
        }

        #region Test Objects

        public class PackageAction1 : IPackageAction
        {
            public bool Execute(string packageName, XmlNode xmlData)
            {
                throw new NotImplementedException();
            }

            public string Alias()
            {
                return "pa1";
            }

            public bool Undo(string packageName, XmlNode xmlData)
            {
                throw new NotImplementedException();
            }

            public XmlNode SampleXml()
            {
                throw new NotImplementedException();
            }
        }

        public class PackageAction2 : IPackageAction
        {
            public bool Execute(string packageName, XmlNode xmlData)
            {
                throw new NotImplementedException();
            }

            public string Alias()
            {
                return "pa2";
            }

            public bool Undo(string packageName, XmlNode xmlData)
            {
                throw new NotImplementedException();
            }

            public XmlNode SampleXml()
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PackageActions;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.UnitTests.TestHelpers;
using Umbraco.Web.Common.Builder;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Composing
{
    [TestFixture]
    public class PackageActionCollectionTests : ComposingTestBase
    {
        [Test]
        public void PackageActionCollectionBuilderWorks()
        {
            var container = TestHelper.GetServiceCollection();

            var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());
 

            var expectedPackageActions = TypeLoader.GetPackageActions();
            composition.WithCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(() => expectedPackageActions);

            var factory = composition.CreateServiceProvider();

            var actions = factory.GetRequiredService<PackageActionCollection>();
            Assert.AreEqual(2, actions.Count());

            // order is unspecified, but both must be there
            var hasAction1 = actions.ElementAt(0) is PackageAction1 || actions.ElementAt(1) is PackageAction1;
            var hasAction2 = actions.ElementAt(0) is PackageAction2 || actions.ElementAt(1) is PackageAction2;

            Assert.IsTrue(hasAction1);
            Assert.IsTrue(hasAction2);
        }

        #region Test Objects

        public class PackageAction1 : IPackageAction
        {
            public bool Execute(string packageName, XElement xmlData)
            {
                throw new NotImplementedException();
            }

            public string Alias()
            {
                return "pa1";
            }

            public bool Undo(string packageName, XElement xmlData)
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
            public bool Execute(string packageName, XElement xmlData)
            {
                throw new NotImplementedException();
            }

            public string Alias()
            {
                return "pa2";
            }

            public bool Undo(string packageName, XElement xmlData)
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

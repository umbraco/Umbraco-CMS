// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PackageActions;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.UnitTests.TestHelpers;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Composing
{
    [TestFixture]
    public class PackageActionCollectionTests : ComposingTestBase
    {
        [Test]
        public void PackageActionCollectionBuilderWorks()
        {
            IServiceCollection container = TestHelper.GetServiceCollection();

            var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

            IEnumerable<Type> expectedPackageActions = TypeLoader.GetPackageActions();
            composition.WithCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(() => expectedPackageActions);

            IServiceProvider factory = composition.CreateServiceProvider();

            PackageActionCollection actions = factory.GetRequiredService<PackageActionCollection>();
            Assert.AreEqual(2, actions.Count());

            // order is unspecified, but both must be there
            bool hasAction1 = actions.ElementAt(0) is PackageAction1 || actions.ElementAt(1) is PackageAction1;
            bool hasAction2 = actions.ElementAt(0) is PackageAction2 || actions.ElementAt(1) is PackageAction2;

            Assert.IsTrue(hasAction1);
            Assert.IsTrue(hasAction2);
        }

        public class PackageAction1 : IPackageAction
        {
            public bool Execute(string packageName, XElement xmlData) => throw new NotImplementedException();

            public string Alias() => "pa1";

            public bool Undo(string packageName, XElement xmlData) => throw new NotImplementedException();

            public XmlNode SampleXml() => throw new NotImplementedException();
        }

        public class PackageAction2 : IPackageAction
        {
            public bool Execute(string packageName, XElement xmlData) => throw new NotImplementedException();

            public string Alias() => "pa2";

            public bool Undo(string packageName, XElement xmlData) => throw new NotImplementedException();

            public XmlNode SampleXml() => throw new NotImplementedException();
        }
    }
}

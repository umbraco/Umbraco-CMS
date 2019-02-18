﻿using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PackageActions;
using Umbraco.Tests.Components;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class PackageActionCollectionTests : ComposingTestBase
    {
        [Test]
        public void PackageActionCollectionBuilderWorks()
        {
            var container = RegisterFactory.Create();
            
            var composition = new Composition(container, new TypeLoader(), Mock.Of<IProfilingLogger>(), ComponentTests.MockRuntimeState(RuntimeLevel.Run));

            composition.WithCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(() => TypeLoader.GetPackageActions());

            Current.Factory = composition.CreateFactory();

            var actions = Current.PackageActions;
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

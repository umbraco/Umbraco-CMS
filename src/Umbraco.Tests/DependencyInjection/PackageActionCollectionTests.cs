using System;
using System.Linq;
using System.Xml;
using LightInject;
using NUnit.Framework;
using Umbraco.Core.DI;
using Umbraco.Core.Plugins;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Tests.DI
{
    [TestFixture]
    public class PackageActionCollectionTests : ResolverBaseTest
	{		
        // NOTE
        // ManyResolverTests ensure that we'll get our actions back and PackageActionResolver works,
        // so all we're testing here is that plugin manager _does_ find our package actions
        // which should be ensured by PlugingManagerTests anyway, so this is useless?
        [Test]
		public void FindAllPackageActions()
		{
            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();

            container.RegisterCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(() => PluginManager.ResolvePackageActions());

			var actions = Current.PackageActions;
			Assert.AreEqual(2, actions.Count());

            // order is unspecified, but both must be there
            bool hasAction1 = actions.ElementAt(0) is PackageAction1 || actions.ElementAt(1) is PackageAction1;
            bool hasAction2 = actions.ElementAt(0) is PackageAction2 || actions.ElementAt(1) is PackageAction2;
            Assert.IsTrue(hasAction1);
            Assert.IsTrue(hasAction2);
        }

		#region Classes for tests

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
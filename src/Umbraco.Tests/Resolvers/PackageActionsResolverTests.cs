using System;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;
using umbraco.interfaces;

namespace Umbraco.Tests.Resolvers
{
	[TestFixture]
	public class PackageActionsResolverTests
	{
		[SetUp]
		public void Initialize()
		{
			TestHelper.SetupLog4NetForTests();

            PackageActionsResolver.Reset();

			// ensures it's reset
			PluginManager.Current = new PluginManager(false);

			// for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
			PluginManager.Current.AssembliesToScan = new[]
				{
					this.GetType().Assembly // this assembly only
				};
		}

		[TearDown]
		public void TearDown()
		{
            PackageActionsResolver.Reset();
            PluginManager.Current = null;
        }
        
        // NOTE
        // ManyResolverTests ensure that we'll get our actions back and PackageActionResolver works,
        // so all we're testing here is that plugin manager _does_ find our package actions
        // which should be ensured by PlugingManagerTests anyway, so this is useless?
        [Test]
		public void FindAllPackageActions()
		{
            PackageActionsResolver.Current = new PackageActionsResolver(
                () => PluginManager.Current.ResolvePackageActions());

            Resolution.Freeze();

			var actions = PackageActionsResolver.Current.PackageActions;
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
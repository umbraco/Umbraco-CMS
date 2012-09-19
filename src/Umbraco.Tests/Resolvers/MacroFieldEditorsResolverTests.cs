using System;
using System.Linq;
using System.Web.UI;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;
using umbraco.interfaces;

namespace Umbraco.Tests.Resolvers
{
	[TestFixture]
	public class MacroFieldEditorsResolverTests
	{
		[SetUp]
		public void Initialize()
		{
			TestHelper.SetupLog4NetForTests();

			//this ensures its reset
			PluginManager.Current = new PluginManager();

			//for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
			PluginManager.Current.AssembliesToScan = new[]
				{
					this.GetType().Assembly
				};

			MacroFieldEditorsResolver.Current = new MacroFieldEditorsResolver(
				PluginManager.Current.ResolveMacroRenderings());

			Resolution.Freeze();
		}

		[TearDown]
		public void TearDown()
		{
			MacroFieldEditorsResolver.Reset();
			Resolution.IsFrozen = false;
			PluginManager.Current.AssembliesToScan = null;
		}

		[Test]
		public void Find_Types()
		{
			var found = MacroFieldEditorsResolver.Current.MacroControlTypes;
			Assert.AreEqual(2, found.Count());
		}

		#region Classes for tests
		public class ControlMacroRendering : Control, IMacroGuiRendering
		{
			public string Value
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public bool ShowCaption
			{
				get { throw new NotImplementedException(); }
			}
		}

		public class NonControlMacroRendering : IMacroGuiRendering
		{
			public string Value
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public bool ShowCaption
			{
				get { throw new NotImplementedException(); }
			}
		}
		#endregion
	}
}
using System;
using System.Linq;
using System.Web.UI;
using NUnit.Framework;
using Umbraco.Core;
using umbraco.editorControls.macrocontainer;
using umbraco.interfaces;

namespace Umbraco.Tests
{
	[TestFixture]
	public class MacroControlFactoryTests
	{
		[SetUp]
		public void Initialize()
		{
			//for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
			PluginTypeResolver.Current.AssembliesToScan = new[]
				{
					this.GetType().Assembly
				};
		}

		[Test]
		public void Find_Types()
		{
			var found = MacroControlFactory.MacroControlTypes;
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
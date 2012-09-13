using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;
using umbraco.BasePages;
using umbraco.interfaces;

namespace Umbraco.Tests
{

	[TestFixture]
	public class ActionsResolverTests
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

			ActionsResolver.Current = new ActionsResolver(
				PluginManager.Current.ResolveActions());

			Resolution.Freeze();
		}

		[TearDown]
		public void TearDown()
		{
			ActionsResolver.Reset();
			Resolution.IsFrozen = false;
		}

		[Test]
		public void Create_Types()
		{
			var found = ActionsResolver.Current.Actions;
			Assert.AreEqual(2, found.Count());
		}		

		#region Classes for tests
		public class SingletonAction : IAction
		{
			//create singleton
			private static readonly SingletonAction instance = new SingletonAction();
			
			public static SingletonAction Instance
			{
				get { return instance; }
			}

			#region IAction Members

			public char Letter
			{
				get
				{
					return 'I';
				}
			}

			public string JsFunctionName
			{
				get
				{
					return string.Format("{0}.actionAssignDomain()", ClientTools.Scripts.GetAppActions);
				}
			}

			public string JsSource
			{
				get
				{
					return null;
				}
			}

			public string Alias
			{
				get
				{
					return "assignDomain";
				}
			}

			public string Icon
			{
				get
				{
					return ".sprDomain";
				}
			}

			public bool ShowInNotifier
			{
				get
				{
					return false;
				}
			}
			public bool CanBePermissionAssigned
			{
				get
				{
					return true;
				}
			}
			#endregion
		}

		public class NonSingletonAction : IAction
		{
			#region IAction Members

			public char Letter
			{
				get
				{
					return 'Q';
				}
			}

			public string JsFunctionName
			{
				get
				{
					return string.Format("{0}.actionAssignDomain()", ClientTools.Scripts.GetAppActions);
				}
			}

			public string JsSource
			{
				get
				{
					return null;
				}
			}

			public string Alias
			{
				get
				{
					return "asfasdf";
				}
			}

			public string Icon
			{
				get
				{
					return ".sprDomain";
				}
			}

			public bool ShowInNotifier
			{
				get
				{
					return false;
				}
			}
			public bool CanBePermissionAssigned
			{
				get
				{
					return true;
				}
			}
			#endregion
		}

		
		#endregion
	}
}
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;
using umbraco.BasePages;
using umbraco.interfaces;

namespace Umbraco.Tests.Resolvers
{
    [TestFixture]
	public class ActionsResolverTests
	{
		[SetUp]
		public void Initialize()
		{
            TestHelper.SetupLog4NetForTests();

            ActionsResolver.Reset();

			// this ensures it's reset
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
            ActionsResolver.Reset();
		    PluginManager.Current = null;
		}

        // NOTE
        // ManyResolverTests ensure that we'll get our actions back and ActionsResolver works,
        // so all we're testing here is that plugin manager _does_ find our actions
        // which should be ensured by PlugingManagerTests anyway, so this is useless?
        // maybe not as it seems to handle the "instance" thing... so we test that we respect the singleton?
        [Test]
		public void FindAllActions()
		{
            ActionsResolver.Current = new ActionsResolver(
                () => PluginManager.Current.ResolveActions());

            Resolution.Freeze();

            var actions = ActionsResolver.Current.Actions;
			Assert.AreEqual(2, actions.Count());

            // order is unspecified, but both must be there
            bool hasAction1 = actions.ElementAt(0) is SingletonAction || actions.ElementAt(1) is SingletonAction;
            bool hasAction2 = actions.ElementAt(0) is NonSingletonAction || actions.ElementAt(1) is NonSingletonAction;
            Assert.IsTrue(hasAction1);
            Assert.IsTrue(hasAction2);

            SingletonAction action = (SingletonAction)(actions.ElementAt(0) is SingletonAction ? actions.ElementAt(0) : actions.ElementAt(1));

            // ensure we respect the singleton
            Assert.AreSame(SingletonAction.Instance, action);
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
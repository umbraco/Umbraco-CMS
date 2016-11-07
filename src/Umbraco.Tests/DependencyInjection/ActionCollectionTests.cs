using System.Linq;
using NUnit.Framework;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Tests.DI
{
    [TestFixture]
    public class ActionCollectionTests : ResolverBaseTest
    {
        // NOTE
        // ManyResolverTests ensure that we'll get our actions back and ActionsResolver works,
        // so all we're testing here is that plugin manager _does_ find our actions
        // which should be ensured by PlugingManagerTests anyway, so this is useless?
        // maybe not as it seems to handle the "instance" thing... so we test that we respect the singleton?
        [Test]
		public void FindAllActions()
        {
            var collectionBuilder = new ActionCollectionBuilder();
            collectionBuilder.SetProducer(() => PluginManager.ResolveActions());

            var actions = collectionBuilder.CreateCollection();
			Assert.AreEqual(2, actions.Count());

            // order is unspecified, but both must be there
            var hasAction1 = actions.ElementAt(0) is SingletonAction || actions.ElementAt(1) is SingletonAction;
            var hasAction2 = actions.ElementAt(0) is NonSingletonAction || actions.ElementAt(1) is NonSingletonAction;
            Assert.IsTrue(hasAction1);
            Assert.IsTrue(hasAction2);

            var action = (SingletonAction)(actions.ElementAt(0) is SingletonAction ? actions.ElementAt(0) : actions.ElementAt(1));

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
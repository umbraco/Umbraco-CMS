using System.Linq;
using NUnit.Framework;
using Umbraco.Web;
using Umbraco.Web.UI.Pages;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class ActionCollectionTests : ComposingTestBase
    {
        [Test]
        public void ActionCollectionBuilderWorks()
        {
            var collectionBuilder = new ActionCollectionBuilder();
            collectionBuilder.SetProducer(() => TypeLoader.GetActions());

            var actions = collectionBuilder.CreateCollection();
            Assert.AreEqual(2, actions.Count());

            // order is unspecified, but both must be there
            var hasAction1 = actions.ElementAt(0) is SingletonAction || actions.ElementAt(1) is SingletonAction;
            var hasAction2 = actions.ElementAt(0) is NonSingletonAction || actions.ElementAt(1) is NonSingletonAction;
            Assert.IsTrue(hasAction1);
            Assert.IsTrue(hasAction2);

            var singletonAction = (SingletonAction) (actions.ElementAt(0) is SingletonAction ? actions.ElementAt(0) : actions.ElementAt(1));

            // ensure it is a singleton
            Assert.AreSame(SingletonAction.Instance, singletonAction);
        }

        #region Test Objects

        public class SingletonAction : IAction
        {
            public static SingletonAction Instance { get; } = new SingletonAction();

            public char Letter => 'I';

            public string JsFunctionName => $"{ClientTools.Scripts.GetAppActions}.actionAssignDomain()";

            public string JsSource => null;

            public string Alias => "assignDomain";

            public string Icon => ".sprDomain";

            public bool ShowInNotifier => false;

            public bool CanBePermissionAssigned => true;
        }

        public class NonSingletonAction : IAction
        {
            public char Letter => 'Q';

            public string JsFunctionName => $"{ClientTools.Scripts.GetAppActions}.actionAssignDomain()";

            public string JsSource => null;

            public string Alias => "asfasdf";

            public string Icon => ".sprDomain";

            public bool ShowInNotifier => false;

            public bool CanBePermissionAssigned => true;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web._Legacy.Actions
{
    internal class ActionCollectionBuilder : ICollectionBuilder<ActionCollection, IAction>
    {
        private static Func<IEnumerable<Type>> _producer;

        public static ActionCollectionBuilder Register(IServiceContainer container)
        {
            // register the builder - per container
            var builderLifetime = new PerContainerLifetime();
            container.Register<ActionCollectionBuilder>(builderLifetime);

            // get the builder, get the collection lifetime
            var builder = container.GetInstance<ActionCollectionBuilder>();
            var collectionLifetime = builder.CollectionLifetime;

            // register the collection - special lifetime
            // the lifetime here is custom ResettablePerContainerLifetime which will manage one
            // single instance of the collection (much alike PerContainerLifetime) but can be resetted
            // to force a new collection to be created.
            // this is needed because of the weird things we do during install, where we'd use the
            // infamous DirtyBackdoorToConfiguration to reset the ActionResolver way after Resolution
            // had frozen. This has been replaced by the possibility here to set the producer at any
            // time - but the builder is internal - and all this will be gone eventually.
            container.Register(factory => factory.GetInstance<ActionCollectionBuilder>().CreateCollection(), collectionLifetime);

            return builder;
        }

        public ActionCollection CreateCollection()
        {
            var actions = new List<IAction>();
            foreach (var type in _producer())
            {
                var getter = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var instance = getter == null
                    ? Activator.CreateInstance(type) as IAction
                    : getter.GetValue(null, null) as IAction;
                if (instance == null) continue;
                actions.Add(instance);
            }
            return new ActionCollection(actions);
        }

        public void SetProducer(Func<IEnumerable<Type>> producer)
        {
            _producer = producer;
            CollectionLifetime.Reset();
        }

        private ResettablePerContainerLifetime CollectionLifetime { get; } = new ResettablePerContainerLifetime();

        private class ResettablePerContainerLifetime : ILifetime
        {
            private object _instance;

            public object GetInstance(Func<object> createInstance, Scope scope)
            {
                // not dealing with disposable instances, actions are not disposable
                return _instance ?? (_instance = createInstance());
            }

            public void Reset()
            {
                _instance = null;
            }
        }
    }

    public class ActionCollection : BuilderCollectionBase<IAction>
    {
        public ActionCollection(IEnumerable<IAction> items)
            : base(items)
        { }

        internal T GetAction<T>()
            where T : IAction
        {
            return this.OfType<T>().SingleOrDefault();
        }
    }

    public interface IAction : IDiscoverable
    {
        char Letter { get; }
        bool ShowInNotifier { get; }
        bool CanBePermissionAssigned { get; }
        string Icon { get; }
        string Alias { get; }
        string JsFunctionName { get; }
        /// <summary>
        /// A path to a supporting JavaScript file for the IAction. A script tag will be rendered out with the reference to the JavaScript file.
        /// </summary>
        string JsSource { get; }
    }

    /// <summary>
    /// This action is invoked when a domain is being assigned to a document
    /// </summary>
    public class ActionAssignDomain : IAction
    {
        /// <summary>
        /// A public constructor exists ONLY for backwards compatibility in regards to 3rd party add-ons.
        /// All Umbraco assemblies should use the singleton instantiation (this.Instance)
        /// When this applicatio is refactored, this constuctor should be made private.
        /// </summary>
        [Obsolete("Use the singleton instantiation instead of a constructor")]
        public ActionAssignDomain() { }

        public static ActionAssignDomain Instance { get; } = new ActionAssignDomain();

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
                return "home";
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
}

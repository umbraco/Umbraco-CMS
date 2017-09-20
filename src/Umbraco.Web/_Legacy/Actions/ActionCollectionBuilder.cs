using System;
using System.Collections.Generic;
using System.Reflection;
using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Web._Legacy.Actions
{
    internal class ActionCollectionBuilder : ICollectionBuilder<ActionCollection, IAction>
    {
        private static Func<IEnumerable<Type>> _producer;

        // for tests only - does not register the collection
        public ActionCollectionBuilder()
        { }

        public ActionCollectionBuilder(IServiceContainer container)
        {
            var collectionLifetime = CollectionLifetime;

            // register the collection - special lifetime
            // the lifetime here is custom ResettablePerContainerLifetime which will manage one
            // single instance of the collection (much alike PerContainerLifetime) but can be resetted
            // to force a new collection to be created.
            // this is needed because of the weird things we do during install, where we'd use the
            // infamous DirtyBackdoorToConfiguration to reset the ActionResolver way after Resolution
            // had frozen. This has been replaced by the possibility here to set the producer at any
            // time - but the builder is internal - and all this will be gone eventually.
            container.Register(factory => factory.GetInstance<ActionCollectionBuilder>().CreateCollection(), collectionLifetime);
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
}

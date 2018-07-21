using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web._Legacy.Actions
{
    internal class ActionCollectionBuilder : ICollectionBuilder<ActionCollection, IAction>
    {
        private Func<IEnumerable<Type>> _producer;
        private ActionCollection _collection;

        // for tests only - does not register the collection
        public ActionCollectionBuilder()
        { }

        public ActionCollectionBuilder(IContainer container)
        {
            // register the collection
            container.RegisterSingleton(factory => factory.GetInstance<ActionCollectionBuilder>().CreateCollection());
        }

        public ActionCollection CreateCollection()
        {
            // create a special collection that can be resetted (ouch)
            return _collection = new ActionCollection(_producer);
        }

        public void SetProducer(Func<IEnumerable<Type>> producer)
        {
            _producer = producer;
            _collection?.Reset(producer);
        }
    }
}

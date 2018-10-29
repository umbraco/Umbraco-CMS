using System;
using System.Collections.Generic;
using System.Reflection;
using LightInject;
using Umbraco.Core.Composing;


namespace Umbraco.Web.Actions
{
    internal class ActionCollectionBuilder : LazyCollectionBuilderBase<ActionCollectionBuilder, ActionCollection, IAction>
    {
        public ActionCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override ActionCollectionBuilder This => this;
    }
}

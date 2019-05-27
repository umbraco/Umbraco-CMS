using Umbraco.Core.Composing;

namespace Umbraco.Web.Actions
{
    internal class ActionCollectionBuilder : LazyCollectionBuilderBase<ActionCollectionBuilder, ActionCollection, IAction>
    {
        protected override ActionCollectionBuilder This => this;
    }
}

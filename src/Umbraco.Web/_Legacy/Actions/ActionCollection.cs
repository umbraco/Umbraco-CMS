using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Web._Legacy.Actions
{
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
}

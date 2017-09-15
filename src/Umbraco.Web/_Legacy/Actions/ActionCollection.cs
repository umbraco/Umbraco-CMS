using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
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

        internal IEnumerable<IAction> GetByLetters(IEnumerable<string> letters)
        {
            var all = this.ToArray();
            return letters.Select(x => all.FirstOrDefault(y => y.Letter.ToString(CultureInfo.InvariantCulture) == x))
                .WhereNotNull()
                .ToArray();
        }
    }
}

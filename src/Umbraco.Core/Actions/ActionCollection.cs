using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Actions
{
    public class ActionCollection : BuilderCollectionBase<IAction>
    {
        public ActionCollection(IEnumerable<IAction> items)
            : base(items)
        { }

        public T GetAction<T>()
            where T : IAction
        {
            return this.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<IAction> GetByLetters(IEnumerable<string> letters)
        {
            var actions = this.ToArray(); // no worry: internally, it's already an array
            return letters
                .Where(x => x.Length == 1)
                .Select(x => actions.FirstOrDefault(y => y.Letter == x[0]))
                .WhereNotNull()
                .ToList();
        }

        public IReadOnlyList<IAction> FromEntityPermission(EntityPermission entityPermission)
        {
            var actions = this.ToArray(); // no worry: internally, it's already an array
            return entityPermission.AssignedPermissions
                .Where(x => x.Length == 1)
                .SelectMany(x => actions.Where(y => y.Letter == x[0]))
                .WhereNotNull()
                .ToList();
        }
    }
}

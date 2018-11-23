using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Membership;


namespace Umbraco.Web.Actions
{
    public class ActionCollection : BuilderCollectionBase<IAction>
    {
        public ActionCollection(IEnumerable<IAction> items)
            : base(items)
        { }

        internal T GetAction<T>()
            where T : IAction
        {
            return this.OfType<T>().FirstOrDefault();
        }

        internal IEnumerable<IAction> GetByLetters(IEnumerable<string> letters)
        {
            return letters
                .Where(x => x.Length == 1)
                .Select(x => this.FirstOrDefault(y => y.Letter == x[0]))
                .WhereNotNull()
                .ToList();
        }

        internal IReadOnlyList<IAction> FromEntityPermission(EntityPermission entityPermission)
        {
            return entityPermission.AssignedPermissions
                .Where(x => x.Length == 1)
                .SelectMany(x => this.Where(y => y.Letter == x[0]))
                .WhereNotNull()
                .ToList();
        }
    }
}

using System.Collections.Generic;
using System.Globalization;
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
            var all = this.ToArray();
            return letters.Select(x => all.FirstOrDefault(y => y.Letter.ToString(CultureInfo.InvariantCulture) == x))
                .WhereNotNull()
                .ToArray();
        }

        internal IReadOnlyList<IAction> FromEntityPermission(EntityPermission entityPermission)
        {
            return entityPermission.AssignedPermissions
                .Where(x => x.Length == 1)
                .Select(x => x.ToCharArray()[0])
                .SelectMany(c => this.Where(x => x.Letter == c))
                .Where(action => action != null)
                .ToList();
        }
    }
}

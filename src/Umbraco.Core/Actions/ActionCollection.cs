// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     The collection of actions
/// </summary>
public class ActionCollection : BuilderCollectionBase<IAction>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ActionCollection" /> class.
    /// </summary>
    public ActionCollection(Func<IEnumerable<IAction>> items)
        : base(items)
    {
    }

    /// <summary>
    ///     Gets the action of the specified type.
    /// </summary>
    /// <typeparam name="T">The specified type to get</typeparam>
    /// <returns>The action</returns>
    public T? GetAction<T>()
        where T : IAction => this.OfType<T>().FirstOrDefault();

    /// <summary>
    ///     Gets the actions by the specified verbs
    /// </summary>
    public ISet<IAction> GetByVerbs(ISet<string> verbs)
    {
        IAction[] actions = this.ToArray(); // no worry: internally, it's already an array
        return verbs
            .Select(x => actions.FirstOrDefault(y => y.Letter == x))
            .WhereNotNull()
            .ToHashSet();
    }

    /// <summary>
    ///     Gets the actions from an EntityPermission
    /// </summary>
    public IReadOnlyList<IAction> FromEntityPermission(EntityPermission entityPermission)
    {
        IAction[] actions = this.ToArray(); // no worry: internally, it's already an array
        return entityPermission.AssignedPermissions
            .SelectMany(x => actions.Where(y => y.Letter == x))
            .WhereNotNull()
            .ToList();
    }
}

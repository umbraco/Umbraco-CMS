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
    ///     Gets the actions by the specified letters
    /// </summary>
    public IEnumerable<IAction> GetByLetters(IEnumerable<string> letters)
    {
        IAction[] actions = this.ToArray(); // no worry: internally, it's already an array
        return letters
            .Where(x => x.Length == 1)
            .Select(x => actions.FirstOrDefault(y => y.Letter == x[0]))
            .WhereNotNull()
            .ToList();
    }

    /// <summary>
    ///     Gets the actions from an EntityPermission
    /// </summary>
    public IReadOnlyList<IAction> FromEntityPermission(EntityPermission entityPermission)
    {
        IAction[] actions = this.ToArray(); // no worry: internally, it's already an array
        return entityPermission.AssignedPermissions
            .Where(x => x.Length == 1)
            .SelectMany(x => actions.Where(y => y.Letter == x[0]))
            .WhereNotNull()
            .ToList();
    }
}

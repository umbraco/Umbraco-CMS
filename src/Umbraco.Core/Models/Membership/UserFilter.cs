using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents filter criteria for querying users.
/// </summary>
public class UserFilter
{
    /// <summary>
    ///     Gets or sets the set of user group keys to include in the filter.
    /// </summary>
    public ISet<Guid>? IncludedUserGroups { get; set; }

    /// <summary>
    ///     Gets or sets the set of user group keys to exclude from the filter.
    /// </summary>
    public ISet<Guid>? ExcludeUserGroups { get; set; }

    /// <summary>
    ///     Gets or sets the set of user states to include in the filter.
    /// </summary>
    public ISet<UserState>? IncludeUserStates { get; set; }

    /// <summary>
    ///     Gets or sets the set of name filters to apply.
    /// </summary>
    public ISet<string>? NameFilters { get; set; }


    /// <summary>
    /// Merges two user filters
    /// </summary>
    /// <param name="target">User filter to merge with.</param>
    /// <returns>A new filter containing the union of the two filters. </returns>
    public UserFilter Merge(UserFilter target) =>
        new UserFilter
        {
            IncludedUserGroups = MergeSet(IncludedUserGroups, target.IncludedUserGroups),
            ExcludeUserGroups = MergeSet(ExcludeUserGroups, target.ExcludeUserGroups),
            IncludeUserStates = MergeSet(IncludeUserStates, target.IncludeUserStates),
            NameFilters = MergeSet(NameFilters, target.NameFilters)
        };

    /// <summary>
    ///     Merges two sets into a single set containing the union of both.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sets.</typeparam>
    /// <param name="source">The source set.</param>
    /// <param name="target">The target set to merge with.</param>
    /// <returns>A new set containing elements from both sets, or null if both are empty.</returns>
    private ISet<T>? MergeSet<T>(ISet<T>? source, ISet<T>? target)
    {
        var set = new HashSet<T>();

        if (source is not null)
        {
            set.UnionWith(source);
        }

        if (target is not null)
        {
            set.UnionWith(target);
        }

        return set.Count == 0 ? null : set;
    }
}

using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Models.Membership;

public class UserFilter
{
    public SortedSet<Guid>? IncludedUserGroups { get; set; }

    public SortedSet<Guid>? ExcludeUserGroups { get; set; }

    public SortedSet<UserState>? IncludeUserStates { get; set; }

    public SortedSet<string>? NameFilters { get; set; }


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

    private SortedSet<T>? MergeSet<T>(SortedSet<T>? source, SortedSet<T>? target)
    {
        var set = new SortedSet<T>();

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

using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Models.Membership;

public class UserFilter
{
    public ISet<Guid>? IncludedUserGroups { get; set; }

    public ISet<Guid>? ExcludeUserGroups { get; set; }

    public ISet<UserState>? IncludeUserStates { get; set; }

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

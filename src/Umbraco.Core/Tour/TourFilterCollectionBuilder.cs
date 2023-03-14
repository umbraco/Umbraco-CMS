using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Tour;

/// <summary>
///     Builds a collection of <see cref="BackOfficeTourFilter" /> items.
/// </summary>
public class TourFilterCollectionBuilder : CollectionBuilderBase<TourFilterCollectionBuilder, TourFilterCollection,
    BackOfficeTourFilter>
{
    private readonly HashSet<BackOfficeTourFilter> _instances = new();

    /// <summary>
    ///     Adds a filter instance.
    /// </summary>
    public void AddFilter(BackOfficeTourFilter filter) => _instances.Add(filter);

    /// <inheritdoc />
    protected override IEnumerable<BackOfficeTourFilter> CreateItems(IServiceProvider factory) =>
        base.CreateItems(factory).Concat(_instances);

    /// <summary>
    ///     Removes a filter instance.
    /// </summary>
    public void RemoveFilter(BackOfficeTourFilter filter) => _instances.Remove(filter);

    /// <summary>
    ///     Removes all filter instances.
    /// </summary>
    public void RemoveAllFilters() => _instances.Clear();

    /// <summary>
    ///     Removes filters matching a condition.
    /// </summary>
    public void RemoveFilter(Func<BackOfficeTourFilter, bool> predicate) =>
        _instances.RemoveWhere(new Predicate<BackOfficeTourFilter>(predicate));

    /// <summary>
    ///     Creates and adds a filter instance filtering by plugin name.
    /// </summary>
    public void AddFilterByPlugin(string pluginName)
    {
        pluginName = pluginName.EnsureStartsWith("^").EnsureEndsWith("$");
        _instances.Add(BackOfficeTourFilter.FilterPlugin(new Regex(pluginName, RegexOptions.IgnoreCase)));
    }

    /// <summary>
    ///     Creates and adds a filter instance filtering by tour filename.
    /// </summary>
    public void AddFilterByFile(string filename)
    {
        filename = filename.EnsureStartsWith("^").EnsureEndsWith("$");
        _instances.Add(BackOfficeTourFilter.FilterFile(new Regex(filename, RegexOptions.IgnoreCase)));
    }
}

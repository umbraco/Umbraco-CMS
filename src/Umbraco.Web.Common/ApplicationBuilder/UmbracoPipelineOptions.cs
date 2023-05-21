using Microsoft.AspNetCore.Builder;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
///     Options to allow modifying the <see cref="IApplicationBuilder" /> pipeline before and after Umbraco registers it's
///     core middlewares.
/// </summary>
public class UmbracoPipelineOptions
{
    /// <summary>
    ///     Returns a mutable list of all registered startup filters
    /// </summary>
    public IList<IUmbracoPipelineFilter> PipelineFilters { get; } = new List<IUmbracoPipelineFilter>();

    /// <summary>
    ///     Adds a filter to the list
    /// </summary>
    /// <param name="filter"></param>
    public void AddFilter(IUmbracoPipelineFilter filter) => PipelineFilters.Add(filter);
}

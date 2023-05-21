namespace Umbraco.Cms.Core.Logging;

/// <summary>
///     Used to render a profiler in a web page
/// </summary>
public interface IProfilerHtml
{
    /// <summary>
    ///     Renders the profiling results.
    /// </summary>
    /// <returns>The profiling results.</returns>
    /// <remarks>Generally used for HTML rendering.</remarks>
    string Render();
}

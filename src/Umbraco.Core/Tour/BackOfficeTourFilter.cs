using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.Tour;

/// <summary>
///     Represents a back-office tour filter.
/// </summary>
public class BackOfficeTourFilter
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeTourFilter" /> class.
    /// </summary>
    /// <param name="pluginName">Value to filter out tours by a plugin, can be null</param>
    /// <param name="tourFileName">Value to filter out a tour file, can be null</param>
    /// <param name="tourAlias">Value to filter out a tour alias, can be null</param>
    /// <remarks>
    ///     Depending on what is null will depend on how the filter is applied.
    ///     If pluginName is not NULL and it's matched then we check if tourFileName is not NULL and it's matched then we check
    ///     tour alias is not NULL and then match it,
    ///     if any steps is NULL then the filters upstream are applied.
    ///     Example, pluginName = "hello", tourFileName="stuff", tourAlias=NULL = we will filter out the tour file "stuff" from
    ///     the plugin "hello" but not from other plugins if the same file name exists.
    ///     Example, tourAlias="test.*" = we will filter out all tour aliases that start with the word "test" regardless of the
    ///     plugin or file name
    /// </remarks>
    public BackOfficeTourFilter(Regex? pluginName, Regex? tourFileName, Regex? tourAlias)
    {
        PluginName = pluginName;
        TourFileName = tourFileName;
        TourAlias = tourAlias;
    }

    /// <summary>
    ///     Gets the plugin name filtering regex.
    /// </summary>
    public Regex? PluginName { get; }

    /// <summary>
    ///     Gets the tour filename filtering regex.
    /// </summary>
    public Regex? TourFileName { get; }

    /// <summary>
    ///     Gets the tour alias filtering regex.
    /// </summary>
    public Regex? TourAlias { get; }

    /// <summary>
    ///     Creates a filter to filter on the plugin name.
    /// </summary>
    public static BackOfficeTourFilter FilterPlugin(Regex pluginName)
        => new(pluginName, null, null);

    /// <summary>
    ///     Creates a filter to filter on the tour filename.
    /// </summary>
    public static BackOfficeTourFilter FilterFile(Regex tourFileName)
        => new(null, tourFileName, null);

    /// <summary>
    ///     Creates a filter to filter on the tour alias.
    /// </summary>
    public static BackOfficeTourFilter FilterAlias(Regex tourAlias)
        => new(null, null, tourAlias);
}

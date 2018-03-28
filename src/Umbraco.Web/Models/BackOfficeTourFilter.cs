using System.Text.RegularExpressions;

namespace Umbraco.Web.Models
{
    public class BackOfficeTourFilter
    {
        public Regex PluginName { get; private set; }
        public Regex TourFileName { get; private set; }
        public Regex TourAlias { get; private set; }

        /// <summary>
        /// Create a filter to filter out a whole plugin's tours
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        public static BackOfficeTourFilter FilterPlugin(Regex pluginName)
        {
            return new BackOfficeTourFilter(pluginName, null, null);
        }

        /// <summary>
        /// Create a filter to filter out a whole tour file
        /// </summary>
        /// <param name="tourFileName"></param>
        /// <returns></returns>
        public static BackOfficeTourFilter FilterFile(Regex tourFileName)
        {
            return new BackOfficeTourFilter(null, tourFileName, null);
        }

        /// <summary>
        /// Create a filter to filter out a tour alias, this will filter out the same alias found in all files
        /// </summary>
        /// <param name="tourAlias"></param>
        /// <returns></returns>
        public static BackOfficeTourFilter FilterAlias(Regex tourAlias)
        {
            return new BackOfficeTourFilter(null, null, tourAlias);
        }

        /// <summary>
        /// Constructor to create a tour filter
        /// </summary>
        /// <param name="pluginName">Value to filter out tours by a plugin, can be null</param>
        /// <param name="tourFileName">Value to filter out a tour file, can be null</param>
        /// <param name="tourAlias">Value to filter out a tour alias, can be null</param>
        /// <remarks>
        /// Depending on what is null will depend on how the filter is applied. 
        /// If pluginName is not NULL and it's matched then we check if tourFileName is not NULL and it's matched then we check tour alias is not NULL and then match it,
        /// if any steps is NULL then the filters upstream are applied.
        /// Example, pluginName = "hello", tourFileName="stuff", tourAlias=NULL = we will filter out the tour file "stuff" from the plugin "hello" but not from other plugins if the same file name exists.
        /// Example, tourAlias="test.*" = we will filter out all tour aliases that start with the word "test" regardless of the plugin or file name
        /// </remarks>
        public BackOfficeTourFilter(Regex pluginName, Regex tourFileName, Regex tourAlias)
        {
            PluginName = pluginName;
            TourFileName = tourFileName;
            TourAlias = tourAlias;
        }
    }
}
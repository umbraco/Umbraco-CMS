using AutoMapper;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Extension methods for AutoMapper's <see cref="ResolutionContext"/>
    /// </summary>
    internal static class ResolutionContextExtensions
    {
        public const string CultureKey = "ContextMapper.Culture";
        
        /// <summary>
        /// Returns the language Id in the mapping context if one is found
        /// </summary>
        /// <param name="resolutionContext"></param>
        /// <returns></returns>
        public static string GetCulture(this ResolutionContext resolutionContext)
        {
            if (!resolutionContext.Options.Items.TryGetValue(CultureKey, out var obj)) return null;

            if (obj is string s)
                return s;

            return null;
        }
    }
}


using System;

namespace Umbraco.Core.Configuration
{
    public class UmbracoVersion
    {
        private static Version _version;

        /// <summary>
        /// Gets the current version of Umbraco.
        /// Version class with the specified major, minor, build (Patch), and revision numbers.
        /// </summary>
        /// <remarks>
        /// CURRENT UMBRACO VERSION ID.
        /// </remarks>
        public static Version Current
        {
            get { return _version ?? (_version = typeof(UmbracoVersion).Assembly.GetName().Version); }
        }

        /// <summary>
        /// Gets the version comment (like beta or RC).
        /// </summary>
        /// <value>The version comment.</value>
        public static string CurrentComment { get { return ""; } }
    }
}
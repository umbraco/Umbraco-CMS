using System;

namespace Umbraco.Core.Configuration
{
    public class UmbracoVersion
    {
        private static readonly Version Version = new Version(6,0,0);

        /// <summary>
        /// Gets the current version of Umbraco.
        /// Version class with the specified major, minor, build (Patch), and revision numbers.
        /// </summary>
        /// <remarks>
        /// CURRENT UMBRACO VERSION ID.
        /// </remarks>
        public static Version Current
        {
            get { return Version; }
        }

        /// <summary>
        /// Gets the version comment (like beta or RC).
        /// </summary>
        /// <value>The version comment.</value>
        public static string CurrentComment { get { return ""; } }
    }
}
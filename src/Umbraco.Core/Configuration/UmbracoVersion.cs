using System;
using System.Reflection;
using Semver;

namespace Umbraco.Core.Configuration
{
    public class UmbracoVersion
    {
        private static readonly Version Version = new Version("7.7.7");

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

        // Get the version of the umbraco.dll by looking at a class in that dll
        // Had to do it like this due to medium trust issues, see: http://haacked.com/archive/2010/11/04/assembly-location-and-medium-trust.aspx
        public static string AssemblyVersion { get { return new AssemblyName(typeof(ActionsResolver).Assembly.FullName).Version.ToString(); } }

        public static SemVersion GetSemanticVersion()
        {
            return new SemVersion(
                Current.Major,
                Current.Minor,
                Current.Build,
                CurrentComment.IsNullOrWhiteSpace() ? null : CurrentComment,
                Current.Revision > 0 ? Current.Revision.ToInvariantString() : null);
        }
    }
}
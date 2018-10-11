using System;
using System.Configuration;
using System.Reflection;
using Semver;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Represents the version of the executing code.
    /// </summary>
    public static class UmbracoVersion
    {
        // BEWARE!
        // This class is parsed and updated by the build scripts.
        // Do NOT modify it unless you understand what you are doing.

        /// <summary>
        /// Gets the version of the executing code.
        /// </summary>
        public static Version Current { get; } = new Version("8.0.0");

        /// <summary>
        /// Gets the version comment of the executing code (eg "beta").
        /// </summary>
        public static string CurrentComment => "alpha.52";

        /// <summary>
        /// Gets the assembly version of Umbraco.Code.dll.
        /// </summary>
        /// <remarks>Get it by looking at a class in that dll, due to medium trust issues,
        /// see http://haacked.com/archive/2010/11/04/assembly-location-and-medium-trust.aspx,
        /// however fixme we don't support medium trust anymore?</remarks>
        public static string AssemblyVersion => new AssemblyName(typeof(UmbracoVersion).Assembly.FullName).Version.ToString();

        /// <summary>
        /// Gets the semantic version of the executing code.
        /// </summary>
        public static SemVersion SemanticVersion { get; } = new SemVersion(
            Current.Major,
            Current.Minor,
            Current.Build,
            CurrentComment.IsNullOrWhiteSpace() ? null : CurrentComment,
            Current.Revision > 0 ? Current.Revision.ToInvariantString() : null);

        /// <summary>
        /// Gets the "local" version of the site.
        /// </summary>
        /// <remarks>
        /// <para>Three things have a version, really: the executing code, the database model,
        /// and the site/files. The database model version is entirely managed via migrations,
        /// and changes during an upgrade. The executing code version changes when new code is
        /// deployed. The site/files version changes during an upgrade.</para>
        /// </remarks>
        public static SemVersion Local
        {
            get
            {
                try
                {
                    // fixme - this should live in its own independent file! NOT web.config!
                    var value = ConfigurationManager.AppSettings["umbracoConfigurationStatus"];
                    return value.IsNullOrWhiteSpace() ? null : SemVersion.TryParse(value, out var semver) ? semver : null;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}

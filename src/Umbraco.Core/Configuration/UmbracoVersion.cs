using System;
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
        public static string CurrentComment => "alpha0020";

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
        public static SemVersion SemanticVersion => new SemVersion(
                Current.Major,
                Current.Minor,
                Current.Build,
                CurrentComment.IsNullOrWhiteSpace() ? null : CurrentComment,
                Current.Revision > 0 ? Current.Revision.ToInvariantString() : null);
    }
}
using System;
using Semver;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Sync;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the state of the Umbraco runtime.
    /// </summary>
    public interface IRuntimeState
    {
        /// <summary>
        /// Gets the version of the executing code.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets the version comment of the executing code.
        /// </summary>
        string VersionComment { get; }

        /// <summary>
        /// Gets the semantic version of the executing code.
        /// </summary>
        SemVersion SemanticVersion { get; }

        /// <summary>
        /// Gets a value indicating whether the application is running in debug mode.
        /// </summary>
        bool Debug { get; }

        /// <summary>
        /// Gets a value indicating whether the runtime is the current main domain.
        /// </summary>
        bool IsMainDom { get; }

        /// <summary>
        /// Get the server's current role.
        /// </summary>
        ServerRole ServerRole { get; }

        /// <summary>
        /// Gets the Umbraco application URL.
        /// </summary>
        /// <remarks>This is eg "http://www.example.com".</remarks>
        Uri ApplicationUrl { get; }

        /// <summary>
        /// Gets the Umbraco application virtual path.
        /// </summary>
        /// <remarks>This is either "/" or eg "/virtual".</remarks>
        string ApplicationVirtualPath { get; }

        /// <summary>
        /// Gets the runtime level of execution.
        /// </summary>
        RuntimeLevel Level { get; }

        /// <summary>
        /// Gets the reason for the runtime level of execution.
        /// </summary>
        RuntimeLevelReason Reason { get; }

        /// <summary>
        /// Gets the current migration state.
        /// </summary>
        string CurrentMigrationState { get; }

        /// <summary>
        /// Gets the final migration state.
        /// </summary>
        string FinalMigrationState { get; }

        /// <summary>
        /// Gets the exception that caused the boot to fail.
        /// </summary>
        BootFailedException BootFailedException { get; }
    }
}

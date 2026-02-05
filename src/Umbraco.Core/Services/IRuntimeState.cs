using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents the state of the Umbraco runtime.
/// </summary>
public interface IRuntimeState
{
    /// <summary>
    ///     Gets the version of the executing code.
    /// </summary>
    Version Version { get; }

    /// <summary>
    ///     Gets the version comment of the executing code.
    /// </summary>
    string VersionComment { get; }

    /// <summary>
    ///     Gets the semantic version of the executing code.
    /// </summary>
    SemVersion SemanticVersion { get; }

    /// <summary>
    ///     Gets the runtime level of execution.
    /// </summary>
    RuntimeLevel Level { get; }

    /// <summary>
    ///     Gets the reason for the runtime level of execution.
    /// </summary>
    RuntimeLevelReason Reason { get; }

    /// <summary>
    ///     Gets the current migration state.
    /// </summary>
    string? CurrentMigrationState { get; }

    /// <summary>
    ///     Gets the final migration state.
    /// </summary>
    string? FinalMigrationState { get; }

    /// <summary>
    ///     Gets the exception that caused the boot to fail.
    /// </summary>
    BootFailedException? BootFailedException { get; }

    /// <summary>
    ///     Returns any state data that was collected during startup
    /// </summary>
    IReadOnlyDictionary<string, object> StartupState { get; }

    /// <summary>
    ///     Determines the runtime level.
    /// </summary>
    void DetermineRuntimeLevel();

    /// <summary>
    ///     Configures the runtime state with the specified level and reason.
    /// </summary>
    /// <param name="level">The runtime level to set.</param>
    /// <param name="reason">The reason for the runtime level.</param>
    /// <param name="bootFailedException">An optional exception that caused a boot failure.</param>
    void Configure(RuntimeLevel level, RuntimeLevelReason reason, Exception? bootFailedException = null);
}

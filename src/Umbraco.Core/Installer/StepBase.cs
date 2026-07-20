using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Installer;

/// <summary>
/// Provides a base class for installation and upgrade steps with common helper methods.
/// </summary>
public abstract class StepBase
{
    /// <summary>
    /// Creates a failed <see cref="Attempt{InstallationResult}"/> with the specified error message.
    /// </summary>
    /// <param name="message">The error message to include in the result.</param>
    /// <returns>A failed <see cref="Attempt{InstallationResult}"/> containing the error message.</returns>
    protected Attempt<InstallationResult> FailWithMessage(string message)
        => Attempt<InstallationResult>.Fail(new InstallationResult { ErrorMessage = message });


    /// <summary>
    /// Creates a successful <see cref="Attempt{InstallationResult}"/>.
    /// </summary>
    /// <returns>A successful <see cref="Attempt{InstallationResult}"/>.</returns>
    protected Attempt<InstallationResult> Success() => Attempt<InstallationResult>.Succeed(new InstallationResult());
}

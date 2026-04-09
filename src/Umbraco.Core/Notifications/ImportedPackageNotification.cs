// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published after a package has been imported.
/// </summary>
/// <remarks>
///     This notification is published after a package has been successfully imported,
///     providing access to the installation summary with details about what was installed.
/// </remarks>
public class ImportedPackageNotification : StatefulNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImportedPackageNotification"/> class.
    /// </summary>
    /// <param name="installationSummary">The summary of the package installation.</param>
    public ImportedPackageNotification(InstallationSummary installationSummary) =>
        InstallationSummary = installationSummary;

    /// <summary>
    ///     Gets the summary of the package installation.
    /// </summary>
    public InstallationSummary InstallationSummary { get; }
}

using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Notifications;

public class ImportedPackageNotification : StatefulNotification
{
    public ImportedPackageNotification(InstallationSummary installationSummary) =>
        InstallationSummary = installationSummary;

    public InstallationSummary InstallationSummary { get; }
}

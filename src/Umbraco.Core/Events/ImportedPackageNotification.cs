using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Events
{
    public class ImportedPackageNotification : INotification
    {

        public ImportedPackageNotification(InstallationSummary installationSummary, IPackageInfo packageMetaData)
        {
            InstallationSummary = installationSummary;
            PackageMetaData = packageMetaData;
        }

        public IPackageInfo PackageMetaData { get; }

        public InstallationSummary InstallationSummary { get; }
    }
}

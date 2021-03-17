using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Events
{
    public class ImportPackageNotification : ICancelableNotification
    {

        public ImportPackageNotification(InstallationSummary installationSummary, IPackageInfo packageMetaData)
        {
            InstallationSummary = installationSummary;
            PackageMetaData = packageMetaData;
        }

        public IPackageInfo PackageMetaData { get; }

        public InstallationSummary InstallationSummary { get; }

        public bool Cancel { get; set; }
    }
}

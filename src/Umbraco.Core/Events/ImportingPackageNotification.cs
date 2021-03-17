using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Events
{
    public class ImportingPackageNotification : ICancelableNotification
    {
        public ImportingPackageNotification(string packageName, IPackageInfo packageMetaData)
        {
            PackageName = packageName;
            PackageMetaData = packageMetaData;
        }

        public string PackageName { get; }

        public IPackageInfo PackageMetaData { get; }

        public bool Cancel { get; set; }
    }
}

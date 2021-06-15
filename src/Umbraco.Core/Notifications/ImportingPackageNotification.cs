using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Notifications
{
    public class ImportingPackageNotification : StatefulNotification, ICancelableNotification
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

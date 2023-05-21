namespace Umbraco.Cms.Core.Notifications;

public class ImportingPackageNotification : StatefulNotification, ICancelableNotification
{
    public ImportingPackageNotification(string packageName) => PackageName = packageName;

    public string PackageName { get; }

    public bool Cancel { get; set; }
}

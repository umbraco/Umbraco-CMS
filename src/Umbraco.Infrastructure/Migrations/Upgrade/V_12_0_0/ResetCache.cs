using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

public class ResetCache : MigrationBase
{
    private readonly IHostingEnvironment _hostingEnvironment;

    public ResetCache(IMigrationContext context, IHostingEnvironment hostingEnvironment)
        : base(context) =>
        _hostingEnvironment = hostingEnvironment;

    protected override void Migrate()
    {
        RebuildCache = true;
        var distCacheFolderAbsolutePath = Path.Combine(_hostingEnvironment.LocalTempPath, "DistCache");
        var nuCacheFolderAbsolutePath = Path.Combine(_hostingEnvironment.LocalTempPath, "NuCache");
        DeleteAllFilesInFolder(distCacheFolderAbsolutePath);
        DeleteAllFilesInFolder(nuCacheFolderAbsolutePath);
    }

    private void DeleteAllFilesInFolder(string path)
    {
        if (Directory.Exists(path) == false)
        {
            return;
        }

        var directoryInfo = new DirectoryInfo(path);

        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            file.Delete();
        }
    }
}

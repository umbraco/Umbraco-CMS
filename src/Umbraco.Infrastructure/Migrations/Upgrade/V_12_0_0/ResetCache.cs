using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

public class ResetCache : MigrationBase
{
    private readonly IHostEnvironment _hostEnvironment;

    public ResetCache(IMigrationContext context, IHostEnvironment hostEnvironment)
        : base(context) => _hostEnvironment = hostEnvironment;

    protected override void Migrate()
    {
        RebuildCache = true;
        var distCacheFolderAbsolutePath = _hostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData + "/DistCache");
        var nuCacheFolderAbsolutePath = _hostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData + "/NuCache");
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
